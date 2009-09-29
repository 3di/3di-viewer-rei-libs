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
using System.Collections.Generic;
using System.Threading;
using OpenMetaverse;
using OpenMetaverse.Utilities;

namespace VoiceTest
{
    public class VoiceException: Exception
    {
        public bool LoggedIn = false;

        public VoiceException(string msg): base(msg) 
        {
        }

        public VoiceException(string msg, bool loggedIn): base(msg) 
        {
            LoggedIn = loggedIn;
        }
    }

    class VoiceTest
    {
        static VoiceManager voice;
        static AutoResetEvent EventQueueRunningEvent = new AutoResetEvent(false);
        static AutoResetEvent ProvisionEvent = new AutoResetEvent(false);
        static AutoResetEvent ParcelVoiceInfoEvent = new AutoResetEvent(false);
        static AutoResetEvent ParticipantPropertiesEvent = new AutoResetEvent(false);
        static string VoiceAccount = String.Empty;
        static string VoicePassword = String.Empty;
        static string VoiceRegionName = String.Empty;
        static int VoiceLocalID = 0;
        static string VoiceChannelURI = String.Empty;
        static string connectorHandle;
        static string accountHandle;
        static string sessionHandle;

        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: VoiceTest.exe [firstname] [lastname] [password]");
                return;
            }

            string firstName = args[0];
            string lastName = args[1];
            string password = args[2];

            Settings.LOG_LEVEL = Helpers.LogLevel.None;

            GridClient client = new GridClient();
            client.Settings.MULTIPLE_SIMS = false;
            client.Settings.LOG_RESENDS = false;
            client.Settings.STORE_LAND_PATCHES = true;
            client.Settings.ALWAYS_DECODE_OBJECTS = true;
            client.Settings.ALWAYS_REQUEST_OBJECTS = true;
            client.Settings.SEND_AGENT_UPDATES = true;
            client.Network.OnEventQueueRunning += client_OnEventQueueRunning;

            string loginURI = client.Settings.LOGIN_SERVER;
            if (4 == args.Length) {
                loginURI = args[3];
            }

            try {

                // Login
                Console.WriteLine("Logging into the grid as " + firstName + " " + lastName + "...");
                LoginParams loginParams =
                    client.Network.DefaultLoginParams(firstName, lastName, password, "Voice Test", "1.0.0");
                loginParams.URI = loginURI;
                if (!client.Network.Login(loginParams))
                    throw new VoiceException("Login to SL failed: " + client.Network.LoginMessage);
                Console.WriteLine("Logged in: " + client.Network.LoginMessage);

                // Create VoiceManager
                voice = new VoiceManager(client);
                voice.OnParticipantProperties += new VoiceManager.ParticipantPropertiesCallback(voice_OnParticipantProperties);
                voice.OnProvisionAccount += voice_OnProvisionAccount;
                voice.OnParcelVoiceInfo += voice_OnParcelVoiceInfo;
                voice.OnSessionTerminated += new VoiceManager.SessionCreatedCallback(voice_OnSessionTerminated);
                voice.OnAccountLogout += new VoiceManager.BasicActionCallback(voice_OnAccountLogout);
                voice.OnConnectorInitiateShutdown += new VoiceManager.BasicActionCallback(voice_OnConnectorInitiateShutdown);
                voice.BlockingTimeout = 5 * 1000;
                voice.OnNewSession += new VoiceManager.NewSessionCallback(voice_OnNewSession);

                //voice.VoiceAccountFromUUID(UUID.Parse("709bd3d8-b28f-f22e-01fc-6d3d1c1f9293"));
                //UUID v = voice.UUIDFromVoiceAccount("xcJvT2LKP8i4B_G09HB-Skw==");

                StartDaemon();

                StopDaemon();

                Console.WriteLine("Exit? Press 'Y' (If another key was pressed, continue.)");
                string res = System.Console.ReadLine();

                if (res != "y")
                {
                    StartDaemon();

                    StopDaemon();
                }

                Console.WriteLine("Request logout.");
                client.Network.Logout();
            }
            catch(Exception e) 
            {
                Console.WriteLine(e.Message);
                if (e is VoiceException && (e as VoiceException).LoggedIn) 
                {
                    client.Network.Logout();
                }
                
            }
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        static void voice_OnNewSession(int cookie, string accountHandle, string eventSessionHandle, int state, string nameString, string uriString)
        {
            Console.WriteLine("SessionTerminate. accountHandle: " + sessionHandle);
            voice.RequestSessionTerminate(sessionHandle);

            Thread.Sleep(3 * 1000);

            sessionHandle = eventSessionHandle;

            voice.RequestSessionConnector(sessionHandle);
        }

        static void voice_OnSessionTerminated(int cookie, int statusCode, string statusString, string sessionHandle)
        {
        }

        static void StartDaemon()
        {
            if (!voice.StartDaemon()) throw new VoiceException("Failed to start to the voice daemon");

            int _port = 44124;
            string _address = "127.0.0.1";

            if (!voice.DaemonJoin(_address, _port)) throw new VoiceException("Failed to connect to the voice daemon");

            List<string> captureDevices = voice.CaptureDevices();

            Console.WriteLine("Capture Devices:");
            for (int i = 0; i < captureDevices.Count; i++)
                Console.WriteLine(String.Format("{0}. \"{1}\"", i, captureDevices[i]));
            Console.WriteLine();

            List<string> renderDevices = voice.RenderDevices();

            Console.WriteLine("Render Devices:");
            for (int i = 0; i < renderDevices.Count; i++)
                Console.WriteLine(String.Format("{0}. \"{1}\"", i, renderDevices[i]));
            Console.WriteLine();

            Console.WriteLine("Creating voice connector...");
            int status;
            connectorHandle = voice.CreateConnector("10.0.1.90", out status);
            if (String.IsNullOrEmpty(connectorHandle))
                throw new VoiceException("Failed to create a voice connector, error code: " + status, true);
            Console.WriteLine("Voice connector handle: " + connectorHandle);


            //Console.WriteLine("Waiting for OnEventQueueRunning");
            //if (!EventQueueRunningEvent.WaitOne(45 * 1000, false))
            //    throw new VoiceException("EventQueueRunning event did not occur", true);
            //Console.WriteLine("EventQueue running");


            Console.WriteLine("Asking the current simulator to create a provisional account...");
            if (!voice.RequestProvisionAccount())
                throw new VoiceException("Failed to request a provisional account", true);
            if (!ProvisionEvent.WaitOne(120 * 1000, false))
                throw new VoiceException("Failed to create a provisional account", true);
            Console.WriteLine("Provisional account created. Username: " + VoiceAccount +
                              ", Password: " + VoicePassword);


            accountHandle = voice.Login(VoiceAccount, VoicePassword, connectorHandle, out status);
            if (String.IsNullOrEmpty(accountHandle))
                throw new VoiceException("Login failed, error code: " + status, true);
            Console.WriteLine("Login succeeded, account handle: " + accountHandle);


            if (!voice.RequestParcelVoiceInfo())
                throw new Exception("Failed to request parcel voice info");
            if (!ParcelVoiceInfoEvent.WaitOne(45 * 1000, false))
                throw new VoiceException("Failed to obtain parcel info voice", true);

            Console.WriteLine("Parcel Voice Info obtained. Region name {0}, local parcel ID {1}, channel URI {2}",
                              VoiceRegionName, VoiceLocalID, VoiceChannelURI);

            sessionHandle = voice.SessionCreate(accountHandle, VoiceChannelURI, "", VoicePassword, true, false, "MD5", out status);
            if (String.IsNullOrEmpty(sessionHandle))
                throw new VoiceException("Session failed, error code: " + status, true);
            Console.WriteLine("Session create to succeeded, session handle: " + sessionHandle);

            if (!ParticipantPropertiesEvent.WaitOne(30 * 1000, false))
                throw new VoiceException("Failed participant properties", true);

            //Console.WriteLine("RequestSet3DPosition.");
            //voice.RequestSet3DPosition(accountHandle, new VoicePosition(), new VoicePosition());

            //Console.WriteLine("RequestMuteLocalMic.");
            //voice.RequestMuteLocalMic(false);

            //Console.WriteLine("RequestMuteLocalSpeaker.");
            //voice.RequestMuteLocalSpeaker(false);

            //Console.WriteLine("RequestSetLocalMicVolume.");
            //voice.RequestSetLocalMicVolume(24);

            //Console.WriteLine("RequestSetLocalSpeakerVolume.");
            //voice.RequestSetLocalSpeakerVolume(24);
        }

        static void StopDaemon()
        {
            Console.WriteLine("Stop the daemon, Please push any button.");
            System.Console.ReadLine();

            int status;

            if (String.IsNullOrEmpty(accountHandle))
                throw new VoiceException("Logout failed, accountHandle is null or empty.", true);

            Console.WriteLine("SessionTerminate. accountHandle: " + sessionHandle);
            voice.RequestSessionTerminate(sessionHandle);

            Console.WriteLine("Logout. accountHandle: " + accountHandle);
            voice.Logout(accountHandle, out status);
            if (status != 200)
                throw new VoiceException("Logout failed, error code: " + status, true);
            accountHandle = string.Empty;

            Console.WriteLine("InitiateShutdown. connectorHandle: " + connectorHandle);
            if (String.IsNullOrEmpty(connectorHandle))
                throw new VoiceException("Failed initiateShutdown, connectorHandle is null or empty.", true);
            voice.InitiateShutdown(connectorHandle, out status);
            if (status != 200)
                throw new VoiceException("InitiateShutdown failed, error code: " + status, true);
            connectorHandle = string.Empty;
            Console.WriteLine("Voice connector handle: " + connectorHandle);

            Console.WriteLine("Stop Daemon.");
            voice.StopDaemon();

        }
        
        static void client_OnEventQueueRunning(Simulator sim) {
            EventQueueRunningEvent.Set();
        }

        static void client_OnLogMessage(string message, Helpers.LogLevel level)
        {
            if (level == Helpers.LogLevel.Warning || level == Helpers.LogLevel.Error)
                Console.WriteLine(level.ToString() + ": " + message);
        }

        static void voice_OnProvisionAccount(string username, string password, string voice_sip_uri_hostname, string voice_account_server_name, string proxy_server_name)
        {
            VoiceAccount = username;
            VoicePassword = password;

            ProvisionEvent.Set();
        }

        static void voice_OnParcelVoiceInfo(string regionName, int localID, string channelURI)
        {
            VoiceRegionName = regionName;
            VoiceLocalID = localID;
            VoiceChannelURI = channelURI;
            //VoiceChannelURI = "sip:xcJvT2LKP8i4B_G09HB-Skw==@bhr.vivox.com";
            //VoiceChannelURI = "sip:xcJvT2LKP8i4B_G09HB-Skw==@10.0.1.90";
            //VoiceChannelURI = "sip:xcJvT2LKP8i4B_G09HB-Skw==";

            ParcelVoiceInfoEvent.Set();
        }

        static void voice_OnParticipantProperties(int cookie, string uriString, int statusCode, string statusString, bool isLocallyMuted, bool isModeratorMuted, bool isSpeaking, int volume, float energy)
        {
            ParticipantPropertiesEvent.Set();
        }

        static void voice_OnAccountLogout(int cookie, int statusCode, string statusString)
        {
        }

        static void voice_OnConnectorInitiateShutdown(int cookie, int statusCode, string statusString)
        {
        }
    }
}
