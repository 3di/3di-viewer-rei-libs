/*
 * Copyright (c) 2006-2008, openmetaverse.org
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the openmetaverse.org nor the names
 *   of its contributors may be used to endorse or promote products derived from
 *   this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Text;
using System.Collections.Generic;
using System.Threading;
using OpenMetaverse.Packets;

namespace OpenMetaverse
{

    #region Structs
    /// <summary>
    /// Holds group information for Avatars such as those you might find in a profile
    /// </summary>
    public struct AvatarGroup
    {
        /// <summary>true of Avatar accepts group notices</summary>
        public bool AcceptNotices;
        /// <summary>Groups Key</summary>
        public UUID GroupID;
        /// <summary>Texture Key for groups insignia</summary>
        public UUID GroupInsigniaID;
        /// <summary>Name of the group</summary>
        public string GroupName;
        /// <summary>Powers avatar has in the group</summary>
        public GroupPowers GroupPowers;
        /// <summary>Avatars Currently selected title</summary>
        public string GroupTitle;
        /// <summary>true of Avatar has chosen to list this in their profile</summary>
        public bool ListInProfile;
    }

    /// <summary>
    /// Holds group information on an individual profile pick
    /// </summary>
    public struct ProfilePick
    {
        public UUID PickID;
        public UUID CreatorID;
        public bool TopPick;
        public UUID ParcelID;
        public string Name;
        public string Desc;
        public UUID SnapshotID;
        public string User;
        public string OriginalName;
        public string SimName;
        public Vector3d PosGlobal;
        public int SortOrder;
        public bool Enabled;
    }
    #endregion

    /// <summary>
    /// Retrieve friend status notifications, and retrieve avatar names and
    /// profiles
    /// </summary>
    public class AvatarManager
    {
        /// <summary>
        /// Triggered when an avatar animation signal is received
        /// </summary>
        /// <param name="avatarID">UUID of the avatar sending the animation</param>
        /// <param name="anims">UUID of the animation, and animation sequence number</param>
        public delegate void AvatarAnimationCallback(UUID avatarID, InternalDictionary<UUID, int> anims);
        /// <summary>
        /// Triggered when AvatarAppearance is received
        /// </summary>
        /// <param name="defaultTexture"></param>
        /// <param name="faceTextures"></param>
        /// <param name="avatarID"></param>
        /// <param name="isTrial"></param>
        /// <param name="visualParams"></param>
        public delegate void AvatarAppearanceCallback(UUID avatarID, bool isTrial, Primitive.TextureEntryFace defaultTexture, Primitive.TextureEntryFace[] faceTextures, List<byte> visualParams);
        /// <summary>
        /// Triggered when a UUIDNameReply is received
        /// </summary>
        /// <param name="names"></param>
        public delegate void AvatarNamesCallback(Dictionary<UUID, string> names);
        /// <summary>
        /// Triggered when a response for avatar interests is returned
        /// </summary>
        /// <param name="avatarID"></param>
        /// <param name="interests"></param>
        public delegate void AvatarInterestsCallback(UUID avatarID, Avatar.Interests interests);
        /// <summary>
        /// Triggered when avatar properties are received (AvatarPropertiesReply)
        /// </summary>
        /// <param name="avatarID"></param>
        /// <param name="properties"></param>
        public delegate void AvatarPropertiesCallback(UUID avatarID, Avatar.AvatarProperties properties);
        /// <summary>
        /// Triggered when an avatar group list is received (AvatarGroupsReply)
        /// </summary>
        /// <param name="avatarID"></param>
        /// <param name="avatarGroups"></param>
        public delegate void AvatarGroupsCallback(UUID avatarID, List<AvatarGroup> avatarGroups);
        /// <summary>
        /// Triggered when a name search reply is received (AvatarPickerReply)
        /// </summary>
        /// <param name="queryID"></param>
        /// <param name="avatars"></param>
        public delegate void AvatarNameSearchCallback(UUID queryID, Dictionary<UUID, string> avatars);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceID"></param>
        /// <param name="targetID"></param>
        /// <param name="targetPos"></param>
        /// <param name="pointType"></param>
        /// <param name="duration"></param>
        /// <param name="id"></param>
        public delegate void PointAtCallback(UUID sourceID, UUID targetID, Vector3d targetPos, 
            PointAtType pointType, float duration, UUID id);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceID"></param>
        /// <param name="targetID"></param>
        /// <param name="targetPos"></param>
        /// <param name="lookType"></param>
        /// <param name="duration"></param>
        /// <param name="id"></param>
        public delegate void LookAtCallback(UUID sourceID, UUID targetID, Vector3d targetPos,
            LookAtType lookType, float duration, UUID id);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="sourceID"></param>
        /// <param name="targetID"></param>
        /// <param name="targetPos"></param>
        /// <param name="duration"></param>
        /// <param name="id"></param>
        public delegate void EffectCallback(EffectType type, UUID sourceID, UUID targetID,
            Vector3d targetPos, float duration, UUID id);
        /// <summary>
        /// Callback returning a dictionary of avatar's picks
        /// </summary>
        /// <param name="avatarid"></param>
        /// <param name="picks"></param>
        public delegate void AvatarPicksCallback(UUID avatarid, Dictionary<UUID, string> picks);
        /// <summary>
        /// Callback returning a details of a specifick pick
        /// </summary>
        /// <param name="pickid"></param>
        /// <param name="pick"></param>
        public delegate void PickInfoCallback(UUID pickid, ProfilePick pick);

        public delegate void VolumeLevelCallback(UUID sourceID, uint localID, float volumeLevel);

        /// <summary></summary>
        public event AvatarAnimationCallback OnAvatarAnimation;
        /// <summary></summary>
        public event AvatarAppearanceCallback OnAvatarAppearance;
        /// <summary></summary>
        public event AvatarNamesCallback OnAvatarNames;
        /// <summary></summary>
        public event AvatarInterestsCallback OnAvatarInterests;
        /// <summary></summary>
        public event AvatarPropertiesCallback OnAvatarProperties;
        /// <summary></summary>
        public event AvatarGroupsCallback OnAvatarGroups;
        /// <summary></summary>
        public event AvatarNameSearchCallback OnAvatarNameSearch;
        /// <summary></summary>
        public event PointAtCallback OnPointAt;
        /// <summary></summary>
        public event LookAtCallback OnLookAt;
        /// <summary></summary>
        public event EffectCallback OnEffect;
        /// <summary></summary>
        public event AvatarPicksCallback OnAvatarPicks;
        /// <summary></summary>
        public event PickInfoCallback OnPickInfo;

        public event VolumeLevelCallback OnVolumeLevel;

        private GridClient Client;

        /// <summary>
        /// Represents other avatars
        /// </summary>
        /// <param name="client"></param>
        public AvatarManager(GridClient client)
        {
            Client = client;

            // Avatar appearance callback
            Client.Network.RegisterCallback(PacketType.AvatarAppearance, new NetworkManager.PacketCallback(AvatarAppearanceHandler));

            // Avatar profile callbacks
            Client.Network.RegisterCallback(PacketType.AvatarPropertiesReply, new NetworkManager.PacketCallback(AvatarPropertiesHandler));
            // Client.Network.RegisterCallback(PacketType.AvatarStatisticsReply, new NetworkManager.PacketCallback(AvatarStatisticsHandler));
            Client.Network.RegisterCallback(PacketType.AvatarInterestsReply, new NetworkManager.PacketCallback(AvatarInterestsHandler));

            // Avatar group callback
            Client.Network.RegisterCallback(PacketType.AvatarGroupsReply, new NetworkManager.PacketCallback(AvatarGroupsHandler));

            // Viewer effect callback
            Client.Network.RegisterCallback(PacketType.ViewerEffect, new NetworkManager.PacketCallback(ViewerEffectHandler));

            // Other callbacks
            Client.Network.RegisterCallback(PacketType.UUIDNameReply, new NetworkManager.PacketCallback(AvatarNameHandler));
            Client.Network.RegisterCallback(PacketType.AvatarPickerReply, new NetworkManager.PacketCallback(AvatarPickerReplyHandler));
	        Client.Network.RegisterCallback(PacketType.AvatarAnimation, new NetworkManager.PacketCallback(AvatarAnimationHandler));

            // Picks callbacks
            Client.Network.RegisterCallback(PacketType.AvatarPicksReply, new NetworkManager.PacketCallback(AvatarPicksHandler));
            Client.Network.RegisterCallback(PacketType.PickInfoReply, new NetworkManager.PacketCallback(PickInfoHandler));
        }

        /// <summary>Tracks the specified avatar on your map</summary>
        /// <param name="preyID">Avatar ID to track</param>
        public void TrackAvatar(UUID preyID)
        {
            TrackAgentPacket p = new TrackAgentPacket();
            p.AgentData.AgentID = Client.Self.AgentID;
            p.AgentData.SessionID = Client.Self.SessionID;
            p.TargetData.PreyID = preyID;
            Client.Network.SendPacket(p);
        }

        /// <summary>
        /// Request a single avatar name
        /// </summary>
        /// <param name="id">The avatar key to retrieve a name for</param>
        public void RequestAvatarName(UUID id)
        {
            Logger.Log("AvatarManager requesting UUID for " + id, Helpers.LogLevel.Debug);
            UUIDNameRequestPacket request = new UUIDNameRequestPacket();
            request.UUIDNameBlock = new UUIDNameRequestPacket.UUIDNameBlockBlock[1];
            request.UUIDNameBlock[0] = new UUIDNameRequestPacket.UUIDNameBlockBlock();
            request.UUIDNameBlock[0].ID = id;

            Client.Network.SendPacket(request);
        }

        /// <summary>
        /// Request a list of avatar names
        /// </summary>
        /// <param name="ids">The avatar keys to retrieve names for</param>
        public void RequestAvatarNames(List<UUID> ids)
        {
            Logger.Log("AvatarManager requesting UUIDs count " + ids.Count, Helpers.LogLevel.Debug);
            if (ids.Count > 0)
            {
                UUIDNameRequestPacket request = new UUIDNameRequestPacket();
                request.UUIDNameBlock = new UUIDNameRequestPacket.UUIDNameBlockBlock[ids.Count];

                for (int i = 0; i < ids.Count; i++)
                {
                    request.UUIDNameBlock[i] = new UUIDNameRequestPacket.UUIDNameBlockBlock();
                    request.UUIDNameBlock[i].ID = ids[i];
                }

                Client.Network.SendPacket(request);
            }
            else
            {
                // not sending request, no ids!
            }

        }

        /// <summary>
        /// Start a request for Avatar Properties
        /// </summary>
        /// <param name="avatarid"></param>
        public void RequestAvatarProperties(UUID avatarid)
        {
            AvatarPropertiesRequestPacket aprp = new AvatarPropertiesRequestPacket();
            
            aprp.AgentData.AgentID = Client.Self.AgentID;
            aprp.AgentData.SessionID = Client.Self.SessionID;
            aprp.AgentData.AvatarID = avatarid;

            Client.Network.SendPacket(aprp);
        }

        /// <summary>
        /// Search for an avatar (first name, last name, and uuid)
        /// </summary>
        /// <param name="name">The name to search for</param>
        /// <param name="queryID">An ID to associate with this query</param>
        public void RequestAvatarNameSearch(string name, UUID queryID)
        {
            AvatarPickerRequestPacket aprp = new AvatarPickerRequestPacket();

            aprp.AgentData.AgentID = Client.Self.AgentID;
            aprp.AgentData.SessionID = Client.Self.SessionID;
            aprp.AgentData.QueryID = queryID;
            aprp.Data.Name = Utils.StringToBytes(name);

            Client.Network.SendPacket(aprp);
        }

        /// <summary>
        /// Start a request for Avatar Picks
        /// </summary>
        /// <param name="avatarid">UUID of the avatar</param>
        public void RequestAvatarPicks(UUID avatarid)
        {
            GenericMessagePacket gmp = new GenericMessagePacket();

            gmp.AgentData.AgentID = Client.Self.AgentID;
            gmp.AgentData.SessionID = Client.Self.SessionID;
            gmp.AgentData.TransactionID = UUID.Zero;

            gmp.MethodData.Method = Utils.StringToBytes("avatarpicksrequest");
            gmp.MethodData.Invoice = UUID.Zero;
            gmp.ParamList = new GenericMessagePacket.ParamListBlock[1];
            gmp.ParamList[0] = new GenericMessagePacket.ParamListBlock();
            gmp.ParamList[0].Parameter = Utils.StringToBytes(avatarid.ToString());

            Client.Network.SendPacket(gmp);
        }

        /// <summary>
        /// Start a request for details of a specific profile pick
        /// </summary>
        /// <param name="avatarid">UUID of the avatar</param>
        /// <param name="pickid">UUID of the profile pick</param>
        public void RequestPickInfo(UUID avatarid, UUID pickid)
        {
            GenericMessagePacket gmp = new GenericMessagePacket();

            gmp.AgentData.AgentID = Client.Self.AgentID;
            gmp.AgentData.SessionID = Client.Self.SessionID;
            gmp.AgentData.TransactionID = UUID.Zero;

            gmp.MethodData.Method = Utils.StringToBytes("pickinforequest");
            gmp.MethodData.Invoice = UUID.Zero;
            gmp.ParamList = new GenericMessagePacket.ParamListBlock[2];
            gmp.ParamList[0] = new GenericMessagePacket.ParamListBlock();
            gmp.ParamList[0].Parameter = Utils.StringToBytes(avatarid.ToString());
            gmp.ParamList[1] = new GenericMessagePacket.ParamListBlock();
            gmp.ParamList[1].Parameter = Utils.StringToBytes(pickid.ToString());

            Client.Network.SendPacket(gmp);
        }

        #region Packet Handlers

        /// <summary>
        /// Process an incoming UUIDNameReply Packet and insert Full Names into the Avatars Dictionary
        /// </summary>
        /// <param name="packet">Incoming Packet to process</param>
        /// <param name="simulator">Unused</param>
        private void AvatarNameHandler(Packet packet, Simulator simulator)
        {
            if (OnAvatarNames != null)
            {
                Dictionary<UUID, string> names = new Dictionary<UUID, string>();
                UUIDNameReplyPacket reply = (UUIDNameReplyPacket)packet;

                foreach (UUIDNameReplyPacket.UUIDNameBlockBlock block in reply.UUIDNameBlock)
                {
                    names[block.ID] = Utils.BytesToString(block.FirstName) +
                        " " + Utils.BytesToString(block.LastName);
                }
                
                OnAvatarNames(names);
            }
        }

        /// <summary>
        /// Process incoming avatar animations
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="sim"></param>
        private void AvatarAnimationHandler(Packet packet, Simulator sim)
        {
            if (OnAvatarAnimation != null)
            {
                AvatarAnimationPacket anims = (AvatarAnimationPacket)packet;

                InternalDictionary<UUID, int> signaledAnims = new InternalDictionary<UUID, int>();
                
                for(int i=0; i < anims.AnimationList.Length; i++)
                    signaledAnims.Add(anims.AnimationList[i].AnimID, anims.AnimationList[i].AnimSequenceID);

                try { OnAvatarAnimation(anims.Sender.ID, signaledAnims); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }

        /// <summary>
        /// Process incoming avatar appearance
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="sim"></param>
        private void AvatarAppearanceHandler(Packet packet, Simulator sim)
        {
            if (OnAvatarAppearance != null)
            {
                AvatarAppearancePacket appearance = (AvatarAppearancePacket)packet;
                sim.ObjectsAvatars.ForEach(delegate(Avatar av)
                {
                    if (av.ID == appearance.Sender.ID)
                    {
                        List<byte> visualParams = new List<byte>();
                        foreach (AvatarAppearancePacket.VisualParamBlock block in appearance.VisualParam)
                        {
                            visualParams.Add(block.ParamValue);
                        }

                        Primitive.TextureEntry textureEntry = new Primitive.TextureEntry(appearance.ObjectData.TextureEntry, 0,
                                appearance.ObjectData.TextureEntry.Length);

                        Primitive.TextureEntryFace defaultTexture = textureEntry.DefaultTexture;
                        Primitive.TextureEntryFace[] faceTextures = textureEntry.FaceTextures;

                        try { OnAvatarAppearance(appearance.Sender.ID, appearance.Sender.IsTrial, defaultTexture, faceTextures, visualParams); }
                        catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                    }
                });
            }
        }

        /// <summary>
        /// Process incoming avatar properties (profile data)
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="sim"></param>
        private void AvatarPropertiesHandler(Packet packet, Simulator sim)
        {
            if (OnAvatarProperties != null)
            {
                AvatarPropertiesReplyPacket reply = (AvatarPropertiesReplyPacket)packet;
                Avatar.AvatarProperties properties = new Avatar.AvatarProperties();

                properties.ProfileImage = reply.PropertiesData.ImageID;
                properties.FirstLifeImage = reply.PropertiesData.FLImageID;
                properties.Partner = reply.PropertiesData.PartnerID;
                properties.AboutText = Utils.BytesToString(reply.PropertiesData.AboutText);
                properties.FirstLifeText = Utils.BytesToString(reply.PropertiesData.FLAboutText);
                properties.BornOn = Utils.BytesToString(reply.PropertiesData.BornOn);
                //properties.CharterMember = Utils.BytesToString(reply.PropertiesData.CharterMember);
                uint charter = Utils.BytesToUInt(reply.PropertiesData.CharterMember);
                if ( charter == 0 ) {
                    properties.CharterMember = "Resident";
                } else if ( charter == 2 ) {
                    properties.CharterMember = "Charter";
                } else if ( charter == 3 ) {
                    properties.CharterMember = "Linden";
                } else {
                    properties.CharterMember = Utils.BytesToString(reply.PropertiesData.CharterMember);
                }
                properties.Flags = (ProfileFlags)reply.PropertiesData.Flags;
                properties.ProfileURL = Utils.BytesToString(reply.PropertiesData.ProfileURL);

                OnAvatarProperties(reply.AgentData.AvatarID, properties);
            }
        }

        /// <summary>
        /// Process incoming Avatar Interests information
        /// </summary>
        private void AvatarInterestsHandler(Packet packet, Simulator simulator)
        {
            if (OnAvatarInterests != null)
            {
                AvatarInterestsReplyPacket airp = (AvatarInterestsReplyPacket)packet;
                Avatar.Interests interests = new Avatar.Interests();

                interests.WantToMask = airp.PropertiesData.WantToMask;
                interests.WantToText = Utils.BytesToString(airp.PropertiesData.WantToText);
                interests.SkillsMask = airp.PropertiesData.SkillsMask;
                interests.SkillsText = Utils.BytesToString(airp.PropertiesData.SkillsText);
                interests.LanguagesText = Utils.BytesToString(airp.PropertiesData.LanguagesText);

                OnAvatarInterests(airp.AgentData.AvatarID, interests);
            }
        }

        private void AvatarGroupsHandler(Packet packet, Simulator simulator)
        {
            if (OnAvatarGroups != null)
            {
                
                AvatarGroupsReplyPacket groups = (AvatarGroupsReplyPacket)packet;
                List<AvatarGroup> avatarGroups = new List<AvatarGroup>(groups.GroupData.Length);
                
                for (int i = 0; i < groups.GroupData.Length; i++)
                {
                    AvatarGroup avatarGroup = new AvatarGroup();

                    avatarGroup.AcceptNotices = groups.GroupData[i].AcceptNotices;
                    avatarGroup.GroupID = groups.GroupData[i].GroupID;
                    avatarGroup.GroupInsigniaID = groups.GroupData[i].GroupInsigniaID;
                    avatarGroup.GroupName = Utils.BytesToString(groups.GroupData[i].GroupName);
                    avatarGroup.GroupPowers = (GroupPowers)groups.GroupData[i].GroupPowers;
                    avatarGroup.GroupTitle = Utils.BytesToString(groups.GroupData[i].GroupTitle);
                    avatarGroup.ListInProfile = groups.NewGroupData.ListInProfile;

                    avatarGroups.Add(avatarGroup);
                }

                try { OnAvatarGroups(groups.AgentData.AvatarID, avatarGroups); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }

        private void AvatarPickerReplyHandler(Packet packet, Simulator simulator)
        {
            if (OnAvatarNameSearch != null)
            {
                AvatarPickerReplyPacket reply = (AvatarPickerReplyPacket)packet;
                Dictionary<UUID, string> avatars = new Dictionary<UUID, string>();

                foreach (AvatarPickerReplyPacket.DataBlock block in reply.Data)
                {
                    avatars[block.AvatarID] = Utils.BytesToString(block.FirstName) +
                        " " + Utils.BytesToString(block.LastName);
                }

                try { OnAvatarNameSearch(reply.AgentData.QueryID, avatars); }
                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
            }
        }

        /// <summary>
        /// Process an incoming effect
        /// </summary>
        private void ViewerEffectHandler(Packet packet, Simulator simulator)
        {
            ViewerEffectPacket effect = (ViewerEffectPacket)packet;

            foreach (ViewerEffectPacket.EffectBlock block in effect.Effect)
            {
                EffectType type = (EffectType)block.Type;

                //Color4 color;
                //if (block.Color.Length == 4)
                //{
                //    color = new Color4(block.Color, 0);
                //}
                //else
                //{
                //    Client.Log("Received a ViewerEffect.EffectBlock.Color array with " + block.Color.Length + 
                //        " bytes", Helpers.LogLevel.Warning);
                //    color = Color4.Black;
                //}

                // Each ViewerEffect type uses it's own custom binary format for additional data. Fun eh?
                switch (type)
                {
                    case EffectType.Text:
                        Logger.Log("Received a ViewerEffect of type " + type.ToString() + ", implement me!",
                            Helpers.LogLevel.Warning, Client);
                        break;
                    case EffectType.Icon:
                        Logger.Log("Received a ViewerEffect of type " + type.ToString() + ", implement me!",
                            Helpers.LogLevel.Warning, Client);
                        break;
                    case EffectType.Connector:
                        Logger.Log("Received a ViewerEffect of type " + type.ToString() + ", implement me!",
                            Helpers.LogLevel.Warning, Client);
                        break;
                    case EffectType.FlexibleObject:
                        Logger.Log("Received a ViewerEffect of type " + type.ToString() + ", implement me!",
                            Helpers.LogLevel.Warning, Client);
                        break;
                    case EffectType.AnimalControls:
                        Logger.Log("Received a ViewerEffect of type " + type.ToString() + ", implement me!",
                            Helpers.LogLevel.Warning, Client);
                        break;
                    case EffectType.AnimationObject:
                        Logger.Log("Received a ViewerEffect of type " + type.ToString() + ", implement me!",
                            Helpers.LogLevel.Warning, Client);
                        break;
                    case EffectType.Cloth:
                        Logger.Log("Received a ViewerEffect of type " + type.ToString() + ", implement me!",
                            Helpers.LogLevel.Warning, Client);
                        break;
                    case EffectType.Glow:
                        Logger.Log("Received a Glow ViewerEffect which is not implemented yet",
                            Helpers.LogLevel.Warning, Client);
                        break;
                    case EffectType.Beam:
                    case EffectType.Point:
                    case EffectType.Trail:
                    case EffectType.Sphere:
                    case EffectType.Spiral:
                    case EffectType.Edit:
                        if (OnEffect != null)
                        {
                            if (block.TypeData.Length == 56)
                            {
                                UUID sourceAvatar = new UUID(block.TypeData, 0);
                                UUID targetObject = new UUID(block.TypeData, 16);
                                Vector3d targetPos = new Vector3d(block.TypeData, 32);

                                try { OnEffect(type, sourceAvatar, targetObject, targetPos, block.Duration, block.ID); }
                                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                            }
                            else
                            {
                                Logger.Log("Received a " + type.ToString() + 
                                    " ViewerEffect with an incorrect TypeData size of " +
                                    block.TypeData.Length + " bytes", Helpers.LogLevel.Warning, Client);
                            }
                        }
                        break;
                    case EffectType.LookAt:
                        if (OnLookAt != null)
                        {
                            if (block.TypeData.Length == 57)
                            {
                                UUID sourceAvatar = new UUID(block.TypeData, 0);
                                UUID targetObject = new UUID(block.TypeData, 16);
                                Vector3d targetPos = new Vector3d(block.TypeData, 32);
                                LookAtType lookAt = (LookAtType)block.TypeData[56];

                                try { OnLookAt(sourceAvatar, targetObject, targetPos, lookAt, block.Duration,
                                    block.ID); }
                                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                            }
                            else
                            {
                                Logger.Log("Received a LookAt ViewerEffect with an incorrect TypeData size of " +
                                    block.TypeData.Length + " bytes", Helpers.LogLevel.Warning, Client);
                            }
                        }
                        break;
                    case EffectType.PointAt:
                        if (OnPointAt != null)
                        {
                            if (block.TypeData.Length == 57)
                            {
                                UUID sourceAvatar = new UUID(block.TypeData, 0);
                                UUID targetObject = new UUID(block.TypeData, 16);
                                Vector3d targetPos = new Vector3d(block.TypeData, 32);
                                PointAtType pointAt = (PointAtType)block.TypeData[56];

                                try { OnPointAt(sourceAvatar, targetObject, targetPos, pointAt, block.Duration, 
                                    block.ID); }
                                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                            }
                            else
                            {
                                Logger.Log("Received a PointAt ViewerEffect with an incorrect TypeData size of " +
                                    block.TypeData.Length + " bytes", Helpers.LogLevel.Warning, Client);
                            }
                        }
                        break;
                    case EffectType.VolumeLevel:
                        if (OnVolumeLevel != null)
                        {
                            if (block.TypeData.Length == 24)
                            {
                                UUID sourceAvatar = new UUID(block.TypeData, 0);
                                float volumeLevel = BitConverter.ToSingle(block.TypeData, 16);
                                uint localID = BitConverter.ToUInt32(block.TypeData, 20);

                                try
                                {
                                    OnVolumeLevel(sourceAvatar, localID, volumeLevel);
                                }
                                catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                            }
                            else
                            {
                                Logger.Log("Received a VoiceLevel ViewerEffect with an incorrect TypeData size of " +
                                    block.TypeData.Length + " bytes", Helpers.LogLevel.Warning, Client);
                            }
                        }
                        break;
                    default:
                        Logger.Log("Received a ViewerEffect with an unknown type " + type, Helpers.LogLevel.Warning, Client);
                        break;
                }
            }
        }

        /// <summary>
        /// Process an incoming list of profile picks
        /// </summary>
        private void AvatarPicksHandler(Packet packet, Simulator simulator)
        {
            if (OnAvatarPicks == null) {
                return;
            }
            AvatarPicksReplyPacket p = (AvatarPicksReplyPacket)packet;
            Dictionary<UUID, string> picks = new Dictionary<UUID,string>();

            foreach (AvatarPicksReplyPacket.DataBlock b in p.Data) {
                picks.Add(b.PickID, Utils.BytesToString(b.PickName));
            }

            try {
                OnAvatarPicks(p.AgentData.TargetID, picks);
            } catch (Exception ex) {
                Logger.Log(ex.Message, Helpers.LogLevel.Error, Client, ex);
            }
        }

        /// <summary>
        /// Process an incoming details of a profile pick
        /// </summary>
        private void PickInfoHandler(Packet packet, Simulator simulator)
        {
            if (OnPickInfo == null) {
                return;
            }

            PickInfoReplyPacket p = (PickInfoReplyPacket)packet;
            ProfilePick ret = new ProfilePick();
            ret.CreatorID = p.Data.CreatorID;
            ret.Desc = Utils.BytesToString(p.Data.Desc);
            ret.Enabled = p.Data.Enabled;
            ret.Name = Utils.BytesToString(p.Data.Name);
            ret.OriginalName = Utils.BytesToString(p.Data.OriginalName);
            ret.ParcelID = p.Data.ParcelID;
            ret.PickID = p.Data.PickID;
            ret.PosGlobal = p.Data.PosGlobal;
            ret.SimName = Utils.BytesToString(p.Data.SimName);
            ret.SnapshotID = p.Data.SnapshotID;
            ret.SortOrder = p.Data.SortOrder;
            ret.TopPick = p.Data.TopPick;
            ret.User = Utils.BytesToString(p.Data.User);

            try {
                OnPickInfo(ret.PickID, ret);
            } catch (Exception ex) {
                Logger.Log(ex.Message, Helpers.LogLevel.Error, Client, ex);
            }
        }

        #endregion Packet Handlers
    }
}
