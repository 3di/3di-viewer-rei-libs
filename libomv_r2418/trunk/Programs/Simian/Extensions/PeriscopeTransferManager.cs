﻿using System;
using System.Collections.Generic;
using System.IO;
using ExtensionLoader;
using OpenMetaverse;
using OpenMetaverse.Imaging;
using OpenMetaverse.Packets;

namespace Simian.Extensions
{
    public class PeriscopeTransferManager
    {
        public const string UPLOAD_DIR = "uploadedAssets";

        Simian server;
        GridClient client;
        Dictionary<ulong, Asset> CurrentUploads = new Dictionary<ulong, Asset>();
        /// <summary>A map from TransactionIDs to AvatarIDs</summary>
        Dictionary<UUID, KeyValuePair<UUID, UUID>> currentDownloads = new Dictionary<UUID, KeyValuePair<UUID, UUID>>();

        public PeriscopeTransferManager(Simian server, GridClient client)
        {
            this.server = server;
            this.client = client;

            client.Assets.OnAssetReceived += new OpenMetaverse.AssetManager.AssetReceivedCallback(Assets_OnAssetReceived);

            server.UDP.RegisterPacketCallback(PacketType.AssetUploadRequest, new PacketCallback(AssetUploadRequestHandler));
            server.UDP.RegisterPacketCallback(PacketType.SendXferPacket, new PacketCallback(SendXferPacketHandler));
            server.UDP.RegisterPacketCallback(PacketType.AbortXfer, new PacketCallback(AbortXferHandler));
            server.UDP.RegisterPacketCallback(PacketType.TransferRequest, new PacketCallback(TransferRequestHandler));
        }

        public void Stop()
        {
        }

        #region Xfer System

        void AssetUploadRequestHandler(Packet packet, Agent agent)
        {
            AssetUploadRequestPacket request = (AssetUploadRequestPacket)packet;
            UUID assetID = UUID.Combine(request.AssetBlock.TransactionID, agent.SecureSessionID);

            // Check if the asset is small enough to fit in a single packet
            if (request.AssetBlock.AssetData.Length != 0)
            {
                // Create a new asset from the completed upload
                Asset asset = CreateAsset((AssetType)request.AssetBlock.Type, assetID, request.AssetBlock.AssetData);
                if (asset == null)
                {
                    Logger.Log("Failed to create asset from uploaded data", Helpers.LogLevel.Warning);
                    return;
                }

                Logger.DebugLog(String.Format("Storing uploaded asset {0} ({1})", assetID, asset.AssetType));

                asset.Temporary = (request.AssetBlock.Tempfile | request.AssetBlock.StoreLocal);

                // Store the asset
                server.Assets.StoreAsset(asset);

                // Send a success response
                AssetUploadCompletePacket complete = new AssetUploadCompletePacket();
                complete.AssetBlock.Success = true;
                complete.AssetBlock.Type = request.AssetBlock.Type;
                complete.AssetBlock.UUID = assetID;
                server.UDP.SendPacket(agent.Avatar.ID, complete, PacketCategory.Inventory);
            }
            else
            {
                // Create a new (empty) asset for the upload
                Asset asset = CreateAsset((AssetType)request.AssetBlock.Type, assetID, null);
                if (asset == null)
                {
                    Logger.Log("Failed to create asset from uploaded data", Helpers.LogLevel.Warning);
                    return;
                }

                Logger.DebugLog(String.Format("Starting upload for {0} ({1})", assetID, asset.AssetType));

                asset.Temporary = (request.AssetBlock.Tempfile | request.AssetBlock.StoreLocal);

                RequestXferPacket xfer = new RequestXferPacket();
                xfer.XferID.DeleteOnCompletion = request.AssetBlock.Tempfile;
                xfer.XferID.FilePath = 0;
                xfer.XferID.Filename = new byte[0];
                xfer.XferID.ID = request.AssetBlock.TransactionID.GetULong();
                xfer.XferID.UseBigPackets = false;
                xfer.XferID.VFileID = asset.AssetID;
                xfer.XferID.VFileType = request.AssetBlock.Type;

                // Add this asset to the current upload list
                lock (CurrentUploads)
                    CurrentUploads[xfer.XferID.ID] = asset;

                server.UDP.SendPacket(agent.Avatar.ID, xfer, PacketCategory.Inventory);
            }
        }

        void SendXferPacketHandler(Packet packet, Agent agent)
        {
            SendXferPacketPacket xfer = (SendXferPacketPacket)packet;

            Asset asset;
            if (CurrentUploads.TryGetValue(xfer.XferID.ID, out asset))
            {
                if (asset.AssetData == null)
                {
                    if (xfer.XferID.Packet != 0)
                    {
                        Logger.Log(String.Format("Received Xfer packet {0} before the first packet!",
                            xfer.XferID.Packet), Helpers.LogLevel.Error);
                        return;
                    }

                    uint size = Utils.BytesToUInt(xfer.DataPacket.Data);
                    asset.AssetData = new byte[size];

                    Buffer.BlockCopy(xfer.DataPacket.Data, 4, asset.AssetData, 0, xfer.DataPacket.Data.Length - 4);

                    // Confirm the first upload packet
                    ConfirmXferPacketPacket confirm = new ConfirmXferPacketPacket();
                    confirm.XferID.ID = xfer.XferID.ID;
                    confirm.XferID.Packet = xfer.XferID.Packet;
                    server.UDP.SendPacket(agent.Avatar.ID, confirm, PacketCategory.Asset);
                }
                else
                {
                    Buffer.BlockCopy(xfer.DataPacket.Data, 0, asset.AssetData, (int)xfer.XferID.Packet * 1000,
                        xfer.DataPacket.Data.Length);

                    // Confirm this upload packet
                    ConfirmXferPacketPacket confirm = new ConfirmXferPacketPacket();
                    confirm.XferID.ID = xfer.XferID.ID;
                    confirm.XferID.Packet = xfer.XferID.Packet;
                    server.UDP.SendPacket(agent.Avatar.ID, confirm, PacketCategory.Asset);

                    if ((xfer.XferID.Packet & (uint)0x80000000) != 0)
                    {
                        // Asset upload finished
                        Logger.DebugLog(String.Format("Completed Xfer upload of asset {0} ({1}", asset.AssetID, asset.AssetType));

                        lock (CurrentUploads)
                            CurrentUploads.Remove(xfer.XferID.ID);

                        server.Assets.StoreAsset(asset);

                        AssetUploadCompletePacket complete = new AssetUploadCompletePacket();
                        complete.AssetBlock.Success = true;
                        complete.AssetBlock.Type = (sbyte)asset.AssetType;
                        complete.AssetBlock.UUID = asset.AssetID;
                        server.UDP.SendPacket(agent.Avatar.ID, complete, PacketCategory.Asset);
                    }
                }
            }
            else
            {
                Logger.DebugLog("Received a SendXferPacket for an unknown upload");
            }
        }

        void AbortXferHandler(Packet packet, Agent agent)
        {
            AbortXferPacket abort = (AbortXferPacket)packet;

            lock (CurrentUploads)
            {
                if (CurrentUploads.ContainsKey(abort.XferID.ID))
                {
                    Logger.DebugLog(String.Format("Aborting Xfer {0}, result: {1}", abort.XferID.ID,
                        (TransferError)abort.XferID.Result));

                    CurrentUploads.Remove(abort.XferID.ID);
                }
                else
                {
                    Logger.DebugLog(String.Format("Received an AbortXfer for an unknown xfer {0}",
                        abort.XferID.ID));
                }
            }
        }

        #endregion Xfer System

        #region Transfer System

        void TransferRequestHandler(Packet packet, Agent agent)
        {
            TransferRequestPacket request = (TransferRequestPacket)packet;

            ChannelType channel = (ChannelType)request.TransferInfo.ChannelType;
            SourceType source = (SourceType)request.TransferInfo.SourceType;

            if (channel == ChannelType.Asset)
            {
                if (source == SourceType.Asset)
                {
                    // Parse the request
                    UUID assetID = new UUID(request.TransferInfo.Params, 0);
                    AssetType type = (AssetType)(sbyte)Utils.BytesToInt(request.TransferInfo.Params, 16);

                    // Check if we have this asset
                    Asset asset;
                    if (server.Assets.TryGetAsset(assetID, out asset))
                    {
                        if (asset.AssetType == type)
                        {
                            TransferToClient(asset, agent, request.TransferInfo.TransferID);
                        }
                        else
                        {
                            Logger.Log(String.Format(
                                "Request for asset {0} with type {1} does not match actual asset type {2}",
                                assetID, type, asset.AssetType), Helpers.LogLevel.Warning);
                        }
                    }
                    else
                    {
                        // Use the bot to try and request this asset
                        lock (currentDownloads)
                        {
                            currentDownloads.Add(client.Assets.RequestAsset(assetID, type, false),
                                new KeyValuePair<UUID, UUID>(agent.Avatar.ID, request.TransferInfo.TransferID));
                        }
                    }
                }
                else if (source == SourceType.SimEstate)
                {
                    UUID agentID = new UUID(request.TransferInfo.Params, 0);
                    UUID sessionID = new UUID(request.TransferInfo.Params, 16);
                    EstateAssetType type = (EstateAssetType)Utils.BytesToInt(request.TransferInfo.Params, 32);

                    Logger.Log("Please implement estate asset transfers", Helpers.LogLevel.Warning);
                }
                else if (source == SourceType.SimInventoryItem)
                {
                    UUID agentID = new UUID(request.TransferInfo.Params, 0);
                    UUID sessionID = new UUID(request.TransferInfo.Params, 16);
                    UUID ownerID = new UUID(request.TransferInfo.Params, 32);
                    UUID taskID = new UUID(request.TransferInfo.Params, 48);
                    UUID itemID = new UUID(request.TransferInfo.Params, 64);
                    UUID assetID = new UUID(request.TransferInfo.Params, 80);
                    AssetType type = (AssetType)(sbyte)Utils.BytesToInt(request.TransferInfo.Params, 96);

                    if (taskID != UUID.Zero)
                    {
                        // Task (prim) inventory request
                        Logger.Log("Please implement task inventory transfers", Helpers.LogLevel.Warning);
                    }
                    else
                    {
                        // Agent inventory request
                        Logger.Log("Please implement agent inventory transfer", Helpers.LogLevel.Warning);
                    }
                }
                else
                {
                    Logger.Log(String.Format(
                        "Received a TransferRequest that we don't know how to handle. Channel: {0}, Source: {1}",
                        channel, source), Helpers.LogLevel.Warning);
                }
            }
            else
            {
                Logger.Log(String.Format(
                    "Received a TransferRequest that we don't know how to handle. Channel: {0}, Source: {1}",
                    channel, source), Helpers.LogLevel.Warning);
            }
        }

        void TransferToClient(Asset asset, Agent agent, UUID transferID)
        {
            Logger.Log(String.Format("Transferring asset {0} ({1})", asset.AssetID, asset.AssetType), Helpers.LogLevel.Info);

            TransferInfoPacket response = new TransferInfoPacket();
            response.TransferInfo = new TransferInfoPacket.TransferInfoBlock();
            response.TransferInfo.TransferID = transferID;
            
            response.TransferInfo.Params = new byte[20];
            Buffer.BlockCopy(asset.AssetID.GetBytes(), 0, response.TransferInfo.Params, 0, 16);
            Buffer.BlockCopy(Utils.IntToBytes((int)asset.AssetType), 0, response.TransferInfo.Params, 16, 4);

            response.TransferInfo.ChannelType = (int)ChannelType.Asset;
            response.TransferInfo.Size = asset.AssetData.Length;
            response.TransferInfo.Status = (int)StatusCode.OK;
            response.TransferInfo.TargetType = (int)TargetType.Unknown; // Doesn't seem to be used by the client

            server.UDP.SendPacket(agent.Avatar.ID, response, PacketCategory.Asset);

            // Transfer system does not wait for ACKs, just sends all of the
            // packets for this transfer out
            const int MAX_CHUNK_SIZE = Settings.MAX_PACKET_SIZE - 100;
            int processedLength = 0;
            int packetNum = 0;
            while (processedLength < asset.AssetData.Length)
            {
                TransferPacketPacket transfer = new TransferPacketPacket();
                transfer.TransferData.ChannelType = (int)ChannelType.Asset;
                transfer.TransferData.TransferID = transferID;
                transfer.TransferData.Packet = packetNum++;

                int chunkSize = Math.Min(asset.AssetData.Length - processedLength, MAX_CHUNK_SIZE);
                transfer.TransferData.Data = new byte[chunkSize];
                Buffer.BlockCopy(asset.AssetData, processedLength, transfer.TransferData.Data, 0, chunkSize);
                processedLength += chunkSize;

                if (processedLength >= asset.AssetData.Length)
                    transfer.TransferData.Status = (int)StatusCode.Done;
                else
                    transfer.TransferData.Status = (int)StatusCode.OK;

                server.UDP.SendPacket(agent.Avatar.ID, transfer, PacketCategory.Asset);
            }
        }

        #endregion Transfer System

        void Assets_OnAssetReceived(AssetDownload transfer, Asset asset)
        {
            KeyValuePair<UUID, UUID> kvp;
            Agent agent;
            if (currentDownloads.TryGetValue(transfer.ID, out kvp))
            {
                currentDownloads.Remove(transfer.ID);

                if (server.Agents.TryGetValue(kvp.Key, out agent))
                {
                    if (transfer.Success)
                    {
                        server.Assets.StoreAsset(asset);
                        TransferToClient(asset, agent, kvp.Value);
                    }
                    else
                    {
                        Logger.Log("Request for missing asset " + transfer.AssetID.ToString(), Helpers.LogLevel.Warning);

                        // Asset not found
                        TransferInfoPacket response = new TransferInfoPacket();
                        response.TransferInfo = new TransferInfoPacket.TransferInfoBlock();
                        response.TransferInfo.TransferID = kvp.Value;

                        response.TransferInfo.Params = new byte[20];
                        Buffer.BlockCopy(transfer.AssetID.GetBytes(), 0, response.TransferInfo.Params, 0, 16);
                        Buffer.BlockCopy(Utils.IntToBytes((int)transfer.AssetType), 0, response.TransferInfo.Params, 16, 4);

                        response.TransferInfo.ChannelType = (int)ChannelType.Asset;
                        response.TransferInfo.Size = 0;
                        response.TransferInfo.Status = (int)StatusCode.UnknownSource;
                        response.TransferInfo.TargetType = (int)TargetType.Unknown;

                        server.UDP.SendPacket(agent.Avatar.ID, response, PacketCategory.Asset);
                    }
                }
                else
                {
                    Logger.Log("Asset transfer finished for an untracked agent, ignoring", Helpers.LogLevel.Warning);
                }
            }
            else
            {
                Logger.Log("Asset transfer finished for an untracked download, ignoring", Helpers.LogLevel.Warning);
            }
        }

        Asset CreateAsset(AssetType type, UUID assetID, byte[] data)
        {
            switch (type)
            {
                case AssetType.Bodypart:
                    return new AssetBodypart(assetID, data);
                case AssetType.Clothing:
                    return new AssetClothing(assetID, data);
                case AssetType.LSLBytecode:
                    return new AssetScriptBinary(assetID, data);
                case AssetType.LSLText:
                    return new AssetScriptText(assetID, data);
                case AssetType.Notecard:
                    return new AssetNotecard(assetID, data);
                case AssetType.Texture:
                    return new AssetTexture(assetID, data);
                case AssetType.Animation:
                    return new AssetAnimation(assetID, data);
                case AssetType.CallingCard:
                case AssetType.Folder:
                case AssetType.Gesture:
                case AssetType.ImageJPEG:
                case AssetType.ImageTGA:
                case AssetType.Landmark:
                case AssetType.LostAndFoundFolder:
                case AssetType.Object:
                case AssetType.RootFolder:
                case AssetType.Simstate:
                case AssetType.SnapshotFolder:
                case AssetType.Sound:
                    return new AssetSound(assetID, data);
                case AssetType.SoundWAV:
                case AssetType.TextureTGA:
                case AssetType.TrashFolder:
                case AssetType.Unknown:
                default:
                    Logger.Log("Asset type " + type.ToString() + " not implemented!", Helpers.LogLevel.Warning);
                    return null;
            }
        }
    }
}
