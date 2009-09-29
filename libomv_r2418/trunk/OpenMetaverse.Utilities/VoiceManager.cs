/*
 * Copyright (c) 2007-2008, openmetaverse.org
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
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Xml;
using System.Threading;
using OpenMetaverse;
using OpenMetaverse.Voice;
using OpenMetaverse.StructuredData;
using OpenMetaverse.Http;

namespace OpenMetaverse.Utilities
{
    public enum VoiceStatus
    {
        StatusLoginRetry,
        StatusLoggedIn,
        StatusJoining,
        StatusJoined,
        StatusLeftChannel,
        BeginErrorStatus,
        ErrorChannelFull,
        ErrorChannelLocked,
        ErrorNotAvailable,
        ErrorUnknown
    }

    public enum VoiceServiceType
    {
        /// <summary>Unknown voice service level</summary>
        Unknown,
        /// <summary>Spatialized local chat</summary>
        TypeA,
        /// <summary>Remote multi-party chat</summary>
        TypeB,
        /// <summary>One-to-one and small group chat</summary>
        TypeC
    }

    public class VoicePosition
    {
        /// <summary>Positional vector of the users position</summary>
        public Vector3d Position;
        /// <summary>Velocity vector of the position</summary>
        public Vector3d Velocity;
        /// <summary>At Orientation (X axis) of the position</summary>
        public Vector3d AtOrientation;
        /// <summary>Up Orientation (Y axis) of the position</summary>
        public Vector3d UpOrientation;
        /// <summary>Left Orientation (Z axis) of the position</summary>
        public Vector3d LeftOrientation;
    }

    public partial class VoiceManager
    {
        private const string REQUEST_TERMINATOR = "\n\n\n";
        private const string EXE_PATH = "SLVoice.exe";
        private const string EXE_ARGS = " -ll -1";
        private const int PROCESS_EXIT_WAIT_TIME = 2 * 1000;
        private const int PROCESS_DEFAULT_PORT = 44124;

        public const int VOICE_MAJOR_VERSION = 1;

        public delegate void LoginStateChangeCallback(int cookie, string accountHandle, int statusCode, string statusString, int state);
        public delegate void NewSessionCallback(int cookie, string accountHandle, string sessionHandle, int state, string nameString, string uriString);
        public delegate void SessionStateChangeCallback(int cookie, string uriString, int statusCode, string statusString, string sessionHandle, int state, bool isChannel, string nameString);
        public delegate void ParticipantStateChangeCallback(int cookie, string uriString, int statusCode, string statusString, int state, string nameString, string displayNameString, int participantType);
        public delegate void ParticipantPropertiesCallback(int cookie, string uriString, int statusCode, string statusString, bool isLocallyMuted, bool isModeratorMuted, bool isSpeaking, int volume, float energy);
        public delegate void AuxAudioPropertiesCallback(int cookie, float energy);
        public delegate void BasicActionCallback(int cookie, int statusCode, string statusString);
        public delegate void ConnectorCreatedCallback(int cookie, int statusCode, string versionID, string statusString, string connectorHandle);
        public delegate void LoginCallback(int cookie, int statusCode, string statusString, string accountHandle);
        public delegate void SessionCreatedCallback(int cookie, int statusCode, string statusString, string sessionHandle);
        public delegate void DevicesCallback(int cookie, int statusCode, string statusString, string currentDevice);
        public delegate void ProvisionAccountCallback(string username, string password, string voice_sip_uri_hostname, string voice_account_server_name, string proxy_server_name);
        public delegate void ParcelVoiceInfoCallback(string regionName, int localID, string channelURI);

        public event EventHandler OnDaemonStarted;
        public event EventHandler OnDaemonStoped;
        public event LoginStateChangeCallback OnLoginStateChange;
        public event NewSessionCallback OnNewSession;
        public event SessionStateChangeCallback OnSessionStateChange;
        public event ParticipantStateChangeCallback OnParticipantStateChange;
        public event ParticipantPropertiesCallback OnParticipantProperties;
        public event AuxAudioPropertiesCallback OnAuxAudioProperties;
        public event ConnectorCreatedCallback OnConnectorCreated;
        public event LoginCallback OnLogin;
        public event SessionCreatedCallback OnSessionCreated;
        public event BasicActionCallback OnSessionConnected;
        public event BasicActionCallback OnAccountLogout;
        public event BasicActionCallback OnConnectorInitiateShutdown;
        public event BasicActionCallback OnAccountChannelGetList;
        public event SessionCreatedCallback OnSessionTerminated;
        public event DevicesCallback OnCaptureDevices;
        public event DevicesCallback OnRenderDevices;
        public event ProvisionAccountCallback OnProvisionAccount;
        public event ParcelVoiceInfoCallback OnParcelVoiceInfo;

        public GridClient Client;
        public bool Enabled;

        protected Voice.TCPPipe _DaemonPipe;
        protected VoiceStatus _Status;
        protected int _CommandCookie = 0;
        protected string _TuningSoundFile = String.Empty;
        protected Dictionary<string, string> _ChannelMap = new Dictionary<string, string>();
        protected List<string> _CaptureDevices = new List<string>();
        protected List<string> _RenderDevices = new List<string>();

        private System.Diagnostics.Process process;

        #region Response Processing Variables

        private bool isEvent = false;
        private bool isChannel = false;
        private bool isLocallyMuted = false;
        private bool isModeratorMuted = false;
        private bool isSpeaking = false;
        private int cookie = 0;
        //private int returnCode = 0;
        private int statusCode = 0;
        private int volume = 0;
        private int state = 0;
        private int participantType = 0;
        private float energy = 0f;
        private string statusString = String.Empty;
        //private string uuidString = String.Empty;
        private string actionString = String.Empty;
        private string versionID = String.Empty;
        private string connectorHandle = String.Empty;
        private string accountHandle = String.Empty;
        private string sessionHandle = String.Empty;
        private string eventTypeString = String.Empty;
        private string uriString = String.Empty;
        private string nameString = String.Empty;
        //private string audioMediaString = String.Empty;
        private string displayNameString = String.Empty;

        #endregion Response Processing Variables

        public VoiceManager(GridClient client)
        {
            Client = client;
            Client.Network.RegisterEventCallback("RequiredVoiceVersion", new Caps.EventQueueCallback(RequiredVoiceVersionEventHandler));

            // Register callback handlers for the blocking functions
            RegisterCallbacks();

            Enabled = true;
        }

        public bool IsDaemonRunning()
        {
            bool flag = false;

            if (_DaemonPipe != null)
            {
                flag = _DaemonPipe.Connected;
            }

            return flag;
        }

        public bool StartDaemon()
        {
            return StartDaemon(EXE_PATH, EXE_ARGS);
        }

        public bool StartDaemon(string _exePath, string _args)
        {
            Process[] processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(_exePath));
            foreach (Process item in processes)
            {
                if (!item.HasExited)
                {
                    item.Kill();
                    item.WaitForExit(PROCESS_EXIT_WAIT_TIME);
                }
            }

            if (System.IO.File.Exists(_exePath))
            {
                process = Process.Start(_exePath, _args);
                process.Exited += new EventHandler(process_Exited);

                if (process != null && OnDaemonStarted != null)
                    OnDaemonStarted(this, EventArgs.Empty);
            }

            return process != null;
        }

        public bool DaemonJoin()
        {
            return DaemonJoin("127.0.0.1", PROCESS_DEFAULT_PORT);
        }

        public bool DaemonJoin(string address, int port)
        {
            _DaemonPipe = new TCPPipe();
            _DaemonPipe.OnDisconnected += new TCPPipe.OnDisconnectedCallback(_DaemonPipe_OnDisconnected);
            _DaemonPipe.OnReceiveLine += new TCPPipe.OnReceiveLineCallback(_DaemonPipe_OnReceiveLine);

            SocketException se = _DaemonPipe.Connect(address, port);

            if (se == null)
            {
                return true;
            }
            else
            {
                Console.WriteLine("Connection failed: " + se.Message);
                return false;
            }
        }

        public void StopDaemon()
        {
            if (process != null)
            {
                process.Kill();
                process.WaitForExit(PROCESS_EXIT_WAIT_TIME);
                process = null;
            }
        }

        public bool IsRunDaemon { get { return process != null && !process.HasExited; } }

        public Dictionary<string, string> GetChannelMap()
        {
            return new Dictionary<string, string>(_ChannelMap);
        }

        public List<string> CurrentCaptureDevices()
        {
            return new List<string>(_CaptureDevices);
        }

        public List<string> CurrentRenderDevices()
        {
            return new List<string>(_RenderDevices);
        }

        #region Public utils.
        public string VoiceAccountFromUUID(UUID id)
        {
            string result = "x" + Convert.ToBase64String(id.GetBytes());
            return result.Replace('+', '-').Replace('/', '_');
        }

        public UUID UUIDFromVoiceAccount(string accountName)
        {
            if (accountName.Length == 25 && accountName[0] == 'x' && accountName[23] == '=' && accountName[24] == '=')
            {
                accountName = accountName.Replace('_', '/').Replace('-', '+').Substring(1);
                byte[] idBytes = Convert.FromBase64String(accountName);

                if (idBytes.Length == 16)
                    return new UUID(idBytes, 0);
                else
                    return UUID.Zero;
            }
            else
            {
                return UUID.Zero;
            }
        }

        public string SIPURIFromVoiceAccount(string account, string voiceServer)
        {
            return String.Format("sip:{0}@{1}", account, voiceServer);
        }
        #endregion

        #region Message parser.
        private void MessagePerse(string _message)
        {
            XmlDocument doc = new XmlDocument();
            doc.InnerXml = _message;

            isEvent = false;
            switch (doc.DocumentElement.Name)
            {
                case "Event":
                    isEvent = true;
                    break;

                case "Response":
                    break;

                case "Request":
                    break;
            }

            eventTypeString = GetAttributeFromID(doc.FirstChild, "type");
            actionString = GetAttributeFromID(doc.FirstChild, "action");

            foreach (XmlNode item in doc.FirstChild.ChildNodes)
            {
                switch (item.Name)
                {
                    case "InputXml":
                        cookie = -1;
                        Int32.TryParse(GetAttributeFromID(doc.FirstChild, "requestId"), out cookie);

                        if (cookie == -1)
                            Logger.Log("VoiceManager._DaemonPipe_OnReceiveLine(): Failed to parse InputXml for the cookie", Helpers.LogLevel.Warning, Client);
                        break;

                    case "Results":
                        ParseResultNode(item);
                        break;

                    default:
                        ParseResultNode(doc.FirstChild);
                        break;
                }
            }

            ProcessEvent();
        }

        private void ParseResultNode(XmlNode _node)
        {
            foreach (XmlNode item in _node.ChildNodes)
            {
                switch (item.Name)
                {
                    case "CaptureDevices":
                        _CaptureDevices.Clear();
                        break;

                    case "RenderDevices":
                        _RenderDevices.Clear();
                        break;

                    case "StatusCode":
                        //statusCode = 200;
                        int.TryParse(item.InnerText, out statusCode);
                        break;

                    case "StatusString":
                        statusString = item.InnerText;
                        break;

                    case "State":
                        int.TryParse(item.InnerText, out state);
                        break;

                    case "VersionID":
                        versionID = item.InnerText;
                        break;

                    case "ConnectorHandle":
                        connectorHandle = item.InnerText;
                        break;

                    case "AccountHandle":
                        accountHandle = item.InnerText;
                        break;

                    case "SessionHandle":
                        sessionHandle = item.InnerText;
                        break;

                    case "URI":
                        uriString = item.InnerText;
                        break;

                    case "IsChannel":
                        bool.TryParse(item.InnerText, out isChannel);
                        break;

                    case "Name":
                        nameString = item.InnerText;
                        break;

                    case "ChannelName":
                        nameString = item.InnerText;
                        break;

                    case "ParticipantURI":
                        uriString = item.InnerText;
                        break;

                    case "DisplayName":
                        displayNameString = item.InnerText;
                        break;

                    case "AccountName":
                        nameString = item.InnerText;
                        break;

                    case "ParticipantType":
                        int.TryParse(item.InnerText, out participantType);
                        break;

                    case "IsLocallyMuted":
                        bool.TryParse(item.InnerText, out isLocallyMuted);
                        break;

                    case "IsModeratorMuted":
                        bool.TryParse(item.InnerText, out isModeratorMuted);
                        break;

                    case "IsSpeaking":
                        bool.TryParse(item.InnerText, out isSpeaking);
                        break;

                    case "Volume":
                        int.TryParse(item.InnerText, out volume);
                        break;

                    case "Energy":
                        float.TryParse(item.InnerText, out energy);
                        break;

                    case "MicEnergy":
                        float.TryParse(item.InnerText, out energy);
                        break;

                    case "ChannelURI":
                        uriString = item.InnerText;
                        break;

                    case "ChannelListResult":
                        _ChannelMap[nameString] = item.InnerText;
                        break;

                    case "CaptureDevice":
                        _CaptureDevices.Add(item.InnerText);
                        break;

                    case "CurrentCaptureDevice":
                        nameString = item.InnerText;
                        break;

                    case "RenderDevice":
                        _RenderDevices.Add(item.InnerText);
                        break;

                    case "CurrentRenderDevice":
                        nameString = item.InnerText;
                        break;
                }
            }
        }

        private string GetAttributeFromID(XmlNode _node, string _id)
        {
            string value = string.Empty;
            XmlAttribute attr = _node.Attributes[_id];

            if (attr == null)
                return value;

            value = attr.Value;

            return value;
        }

        private void ProcessEvent()
        {
            if (isEvent)
            {
                switch (eventTypeString)
                {
                    case "LoginStateChangeEvent":
                        if (OnLoginStateChange != null) OnLoginStateChange(cookie, accountHandle, statusCode, statusString, state);
                        break;
                    case "SessionNewEvent":
                        if (OnNewSession != null) OnNewSession(cookie, accountHandle, sessionHandle, state, nameString, uriString);
                        break;
                    case "SessionStateChangeEvent":
                        if (OnSessionStateChange != null) OnSessionStateChange(cookie, uriString, statusCode, statusString, sessionHandle, state, isChannel, nameString);
                        break;
                    case "ParticipantStateChangeEvent":
                        if (OnParticipantStateChange != null) OnParticipantStateChange(cookie, uriString, statusCode, statusString, state, nameString, displayNameString, participantType);
                        break;
                    case "ParticipantPropertiesEvent":
                        if (OnParticipantProperties != null) OnParticipantProperties(cookie, uriString, statusCode, statusString, isLocallyMuted, isModeratorMuted, isSpeaking, volume, energy);
                        break;
                    case "AuxAudioPropertiesEvent":
                        if (OnAuxAudioProperties != null) OnAuxAudioProperties(cookie, energy);
                        break;
                }
            }
            else
            {
                switch (actionString)
                {
                    case "Connector.Create.1":
                        if (OnConnectorCreated != null) OnConnectorCreated(cookie, statusCode, versionID, statusString, connectorHandle);
                        break;
                    case "Account.Login.1":
                        if (OnLogin != null) OnLogin(cookie, statusCode, statusString, accountHandle);
                        break;
                    case "Session.Create.1":
                        if (OnSessionCreated != null) OnSessionCreated(cookie, statusCode, statusString, sessionHandle);
                        break;
                    case "Session.Connect.1":
                        if (OnSessionConnected != null) OnSessionConnected(cookie, statusCode, statusString);
                        break;
                    case "Session.Terminate.1":
                        if (OnSessionTerminated != null) OnSessionTerminated(cookie, statusCode, statusString, sessionHandle);
                        break;
                    case "Account.Logout.1":
                        if (OnAccountLogout != null) OnAccountLogout(cookie, statusCode, statusString);
                        break;
                    case "Connector.InitiateShutdown.1":
                        if (OnConnectorInitiateShutdown != null) OnConnectorInitiateShutdown(cookie, statusCode, statusString);
                        break;
                    case "Account.ChannelGetList.1":
                        if (OnAccountChannelGetList != null) OnAccountChannelGetList(cookie, statusCode, statusString);
                        break;
                    case "Aux.GetCaptureDevices.1":
                        if (OnCaptureDevices != null) OnCaptureDevices(cookie, statusCode, statusString, nameString);
                        break;
                    case "Aux.GetRenderDevices.1":
                        if (OnRenderDevices != null) OnRenderDevices(cookie, statusCode, statusString, nameString);
                        break;
                }
            }
        }

        #endregion Message parser.

        #region Request.
        public int RequestCaptureDevices()
        {
            if (_DaemonPipe.Connected)
            {
                _DaemonPipe.SendData(Encoding.ASCII.GetBytes(String.Format(
                    "<Request requestId=\"{0}\" action=\"Aux.GetCaptureDevices.1\"></Request>{1}",
                    _CommandCookie++,
                    REQUEST_TERMINATOR)));

                return _CommandCookie - 1;
            }
            else
            {
                Logger.Log("VoiceManager.RequestCaptureDevices() called when the daemon pipe is disconnected", Helpers.LogLevel.Error, Client);
                return -1;
            }
        }

        public int RequestRenderDevices()
        {
            if (_DaemonPipe.Connected)
            {
                _DaemonPipe.SendData(Encoding.ASCII.GetBytes(String.Format(
                    "<Request requestId=\"{0}\" action=\"Aux.GetRenderDevices.1\"></Request>{1}",
                    _CommandCookie++,
                    REQUEST_TERMINATOR)));

                return _CommandCookie - 1;
            }
            else
            {
                Logger.Log("VoiceManager.RequestRenderDevices() called when the daemon pipe is disconnected", Helpers.LogLevel.Error, Client);
                return -1;
            }
        }

        public int RequestCreateConnector(string _voiceServer, string _proxyServer, bool _logEnabled, string _logNamePrefix, string _logNameSuffix, int _logLevel)
        {
            if (_DaemonPipe.Connected)
            {
                //string accountServer = String.Format("https://www.{0}/api2/", _voiceServer);
                string accountServer = _voiceServer;
                string proxyServer = _proxyServer;
                string logPath = ".";

                StringBuilder request = new StringBuilder();
                request.Append(String.Format("<Request requestId=\"{0}\" action=\"Connector.Create.1\">", _CommandCookie++));
                request.Append("<ClientName>V2 SDK</ClientName>");
                request.Append(String.Format("<AccountManagementServer>{0}</AccountManagementServer>", accountServer));
                request.Append(String.Format("<ProxyManagementServer>{0}</ProxyManagementServer>", proxyServer));
                request.Append("<Logging>");
                request.Append(String.Format("<Enabled>{0}</Enabled>", _logEnabled.ToString().ToLower()));
                request.Append(String.Format("<Folder>{0}</Folder>", logPath));
                request.Append(String.Format("<FileNamePrefix>{0}</FileNamePrefix>", _logNamePrefix));
                request.Append(String.Format("<FileNameSuffix>{0}</FileNameSuffix>", _logNameSuffix));
                request.Append(String.Format("<LogLevel>{0}</LogLevel>", _logLevel.ToString()));
                request.Append("</Logging>");
                request.Append("</Request>");
                request.Append(REQUEST_TERMINATOR);

                _DaemonPipe.SendData(Encoding.ASCII.GetBytes(request.ToString()));

                return _CommandCookie - 1;
            }
            else
            {
                Logger.Log("VoiceManager.CreateConnector() called when the daemon pipe is disconnected", Helpers.LogLevel.Error, Client);
                return -1;
            }
        }

        private bool RequestVoiceInternal(string me, CapsClient.CompleteCallback callback, string capsName)
        {
            if (Enabled && Client.Network.Connected)
            {
                if (Client.Network.CurrentSim != null && Client.Network.CurrentSim.Caps != null)
                {
                    Uri url = Client.Network.CurrentSim.Caps.CapabilityURI(capsName);

                    if (url != null)
                    {
                        CapsClient request = new CapsClient(url);
                        OSDMap body = new OSDMap();
                        request.OnComplete += new CapsClient.CompleteCallback(callback);
                        request.StartRequest(body);

                        return true;
                    }
                    else
                    {
                        Logger.Log("VoiceManager." + me + "(): " + capsName + " capability is missing", 
                                   Helpers.LogLevel.Info, Client);
                        return false;
                    }
                }
            }

            Logger.Log("VoiceManager.RequestVoiceInternal(): Voice system is currently disabled", 
                       Helpers.LogLevel.Info, Client);
            return false;
            
        }

        public bool RequestProvisionAccount()
        {
            return RequestVoiceInternal("RequestProvisionAccount", ProvisionCapsResponse, "ProvisionVoiceAccountRequest");
        }

        public bool RequestParcelVoiceInfo()
        {
            return RequestVoiceInternal("RequestParcelVoiceInfo", ParcelVoiceInfoResponse, "ParcelVoiceInfoRequest");
        }

        public int RequestLogin(string accountName, string password, string accountURI, string connectorHandle, int _participantPropertyFrequency)
        {
            if (_DaemonPipe.Connected)
            {
                StringBuilder request = new StringBuilder();
                request.Append(String.Format("<Request requestId=\"{0}\" action=\"Account.Login.1\">", _CommandCookie++));
                request.Append(String.Format("<ConnectorHandle>{0}</ConnectorHandle>", connectorHandle));
                request.Append(String.Format("<AccountName>{0}</AccountName>", accountName));
                request.Append(String.Format("<AccountPassword>{0}</AccountPassword>", password));
                request.Append("<AudioSessionAnswerMode>VerifyAnswer</AudioSessionAnswerMode>");
                request.Append("<AccountURI>"+accountURI + "</AccountURI>");
                request.Append(String.Format("<ParticipantPropertyFrequency>{0}</ParticipantPropertyFrequency>", _participantPropertyFrequency));
                request.Append("<EnableBuddiesAndPresence>false</EnableBuddiesAndPresence>");
                request.Append("</Request>");
                request.Append(REQUEST_TERMINATOR);

                _DaemonPipe.SendData(Encoding.ASCII.GetBytes(request.ToString()));

                return _CommandCookie - 1;
            }
            else
            {
                Logger.Log("VoiceManager.Login() called when the daemon pipe is disconnected", Helpers.LogLevel.Error, Client);
                return -1;
            }
        }

        public int RequestSessionCreate(string AccountHandle, string URI, string Name, string Password,
            bool JoinAudio, bool JoinText, string PasswordHashAlgorithm, int type)
        {
            if (_DaemonPipe.Connected)
            {
                StringBuilder request = new StringBuilder();
                request.Append(String.Format("<Request requestId=\"{0}\" action=\"Session.Create.1\">", _CommandCookie++));
                request.Append(String.Format("<AccountHandle>{0}</AccountHandle>", AccountHandle));
                request.Append(String.Format("<URI>{0}</URI>", URI));
                request.Append(String.Format("<JoinAudio>{0}</JoinAudio>", JoinAudio));
                request.Append(String.Format("<JoinText>{0}</JoinText>", JoinText));
                request.Append(String.Format("<Password>{0}</Password>", Password));
                request.Append(String.Format("<Name>{0}</Name>", Name));
                request.Append(String.Format("<PasswordHashAlgorithm>{0}</PasswordHashAlgorithm>", PasswordHashAlgorithm));
                request.Append(String.Format("<Type>{0}</Type>", type));
                request.Append("</Request>");
                request.Append(REQUEST_TERMINATOR);

                _DaemonPipe.SendData(Encoding.ASCII.GetBytes(request.ToString()));

                return _CommandCookie - 1;
            }
            else
            {
                Logger.Log("VoiceManager.SessionCreate() called when the daemon pipe is disconnected", Helpers.LogLevel.Error, Client);
                return -1;
            }
        }

        public int RequestSessionConnector(string SessionHandle)
        {
            if (_DaemonPipe.Connected)
            {
                StringBuilder request = new StringBuilder();
                request.Append(String.Format("<Request requestId=\"{0}\" action=\"Session.Connect.1\">", _CommandCookie++));
                request.Append(String.Format("<SessionHandle>{0}</SessionHandle>", SessionHandle));
                request.Append("<AudioMedia>default</AudioMedia>");
                request.Append("</Request>");
                request.Append(REQUEST_TERMINATOR);

                _DaemonPipe.SendData(Encoding.ASCII.GetBytes(request.ToString()));

                return _CommandCookie - 1;
            }
            else
            {
                Logger.Log("VoiceManager.SessionCreate() called when the daemon pipe is disconnected", Helpers.LogLevel.Error, Client);
                return -1;
            }
        }

        public int RequestSessionTerminate(string SessionHandle)
        {
            if (_DaemonPipe.Connected)
            {
                StringBuilder request = new StringBuilder();
                request.Append(String.Format("<Request requestId=\"{0}\" action=\"Session.Terminate.1\">", _CommandCookie++));
                request.Append(String.Format("<SessionHandle>{0}</SessionHandle>", SessionHandle));
                request.Append("</Request>");
                request.Append(REQUEST_TERMINATOR);

                _DaemonPipe.SendData(Encoding.ASCII.GetBytes(request.ToString()));

                return _CommandCookie - 1;
            }
            else
            {
                Logger.Log("VoiceManager.RequestSessionTerminate() called when the daemon pipe is disconnected", Helpers.LogLevel.Error, Client);
                return -1;
            }
        }

        public int RequestLogout(string accountHandle)
        {
            if (_DaemonPipe.Connected)
            {
                StringBuilder request = new StringBuilder();
                request.Append(String.Format("<Request requestId=\"{0}\" action=\"Account.Logout.1\">", _CommandCookie++));
                request.Append(String.Format("<AccountHandle>{0}</AccountHandle>", accountHandle));
                request.Append("</Request>");
                request.Append(REQUEST_TERMINATOR);

                _DaemonPipe.SendData(Encoding.ASCII.GetBytes(request.ToString()));

                return _CommandCookie - 1;
            }
            else
            {
                Logger.Log("VoiceManager.Logout() called when the daemon pipe is disconnected", Helpers.LogLevel.Error, Client);
                return -1;
            }
        }

        public int RequestInitiateShutdown(string connectorHandle)
        {
            if (_DaemonPipe.Connected)
            {
                StringBuilder request = new StringBuilder();
                request.Append(String.Format("<Request requestId=\"{0}\" action=\"Connector.InitiateShutdown.1\">", _CommandCookie++));
                request.Append(String.Format("<ConnectorHandle>{0}</ConnectorHandle>", connectorHandle));
                request.Append("</Request>");
                request.Append(REQUEST_TERMINATOR);

                _DaemonPipe.SendData(Encoding.ASCII.GetBytes(request.ToString()));

                return _CommandCookie - 1;
            }
            else
            {
                Logger.Log("VoiceManager.CreateConnector() called when the daemon pipe is disconnected", Helpers.LogLevel.Error, Client);
                return -1;
            }
        }

        public int RequestSetRenderDevice(string deviceName)
        {
            if (_DaemonPipe.Connected)
            {
                _DaemonPipe.SendData(Encoding.ASCII.GetBytes(String.Format(
                    "<Request requestId=\"{0}\" action=\"Aux.SetRenderDevice.1\"><RenderDeviceSpecifier>{1}</RenderDeviceSpecifier></Request>{2}",
                    _CommandCookie, deviceName, REQUEST_TERMINATOR)));

                return _CommandCookie - 1;
            }
            else
            {
                Logger.Log("VoiceManager.RequestSetRenderDevice() called when the daemon pipe is disconnected", Helpers.LogLevel.Error, Client);
                return -1;
            }
        }

        public int RequestStartTuningMode(int duration)
        {
            if (_DaemonPipe.Connected)
            {
                _DaemonPipe.SendData(Encoding.ASCII.GetBytes(String.Format(
                    "<Request requestId=\"{0}\" action=\"Aux.CaptureAudioStart.1\"><Duration>{1}</Duration></Request>{2}",
                    _CommandCookie, duration, REQUEST_TERMINATOR)));

                return _CommandCookie - 1;
            }
            else
            {
                Logger.Log("VoiceManager.RequestStartTuningMode() called when the daemon pipe is disconnected", Helpers.LogLevel.Error, Client);
                return -1;
            }
        }

        public int RequestStopTuningMode()
        {
            if (_DaemonPipe.Connected)
            {
                _DaemonPipe.SendData(Encoding.ASCII.GetBytes(String.Format(
                    "<Request requestId=\"{0}\" action=\"Aux.CaptureAudioStop.1\"></Request>{1}",
                    _CommandCookie, REQUEST_TERMINATOR)));

                return _CommandCookie - 1;
            }
            else
            {
                Logger.Log("VoiceManager.RequestStopTuningMode() called when the daemon pipe is disconnected", Helpers.LogLevel.Error, Client);
                return _CommandCookie - 1;
            }
        }

        /// <summary>
        /// Set the combined speaking and listening position in 3D space.
        /// There appears to be no response to this request.
        /// </summary>
        /// <param name="SessionHandle">Handle returned from successful Session ëcreateÅErequest or a SessionNewEvent</param>
        /// <param name="SpeakerPosition">Speaking position</param>
        /// <param name="ListenerPosition">Listening position</param>
        /// <returns></returns>
        public int RequestSet3DPosition(string SessionHandle, VoicePosition SpeakerPosition, VoicePosition ListenerPosition)
        {
            if (_DaemonPipe.Connected)
            {
                StringBuilder request = new StringBuilder();
                request.Append(String.Format("<Request requestId=\"{0}\" action=\"Session.Set3DPosition.1\">", _CommandCookie++));
                request.Append(String.Format("<SessionHandle>{0}</SessionHandle>", SessionHandle));
                request.Append("<OrientationType>legacy</OrientationType>");
                request.Append("<SpeakerPosition>");
                request.Append("<Position>");
                request.Append(String.Format("<X>{0}</X>", SpeakerPosition.Position.X.ToString()));
                request.Append(String.Format("<Y>{0}</Y>", SpeakerPosition.Position.Y.ToString()));
                request.Append(String.Format("<Z>{0}</Z>", SpeakerPosition.Position.Z.ToString()));
                request.Append("</Position>");
                request.Append("<Velocity>");
                request.Append(String.Format("<X>{0}</X>", SpeakerPosition.Velocity.X.ToString()));
                request.Append(String.Format("<Y>{0}</Y>", SpeakerPosition.Velocity.Y.ToString()));
                request.Append(String.Format("<Z>{0}</Z>", SpeakerPosition.Velocity.Z.ToString()));
                request.Append("</Velocity>");
                request.Append("<AtOrientation>");
                request.Append(String.Format("<X>{0}</X>", SpeakerPosition.AtOrientation.X.ToString()));
                request.Append(String.Format("<Y>{0}</Y>", SpeakerPosition.AtOrientation.Y.ToString()));
                request.Append(String.Format("<Z>{0}</Z>", SpeakerPosition.AtOrientation.Z.ToString()));
                request.Append("</AtOrientation>");
                request.Append("<UpOrientation>");
                request.Append(String.Format("<X>{0}</X>", SpeakerPosition.UpOrientation.X.ToString()));
                request.Append(String.Format("<Y>{0}</Y>", SpeakerPosition.UpOrientation.Y.ToString()));
                request.Append(String.Format("<Z>{0}</Z>", SpeakerPosition.UpOrientation.Z.ToString()));
                request.Append("</UpOrientation>");
                request.Append("<LeftOrientation>");
                request.Append(String.Format("<X>{0}</X>", SpeakerPosition.LeftOrientation.X.ToString()));
                request.Append(String.Format("<Y>{0}</Y>", SpeakerPosition.LeftOrientation.Y.ToString()));
                request.Append(String.Format("<Z>{0}</Z>", SpeakerPosition.LeftOrientation.Z.ToString()));
                request.Append("</LeftOrientation>");
                request.Append("</SpeakerPosition>");
                request.Append("<ListenerPosition>");
                request.Append("<Position>");
                request.Append(String.Format("<X>{0}</X>", ListenerPosition.Position.X.ToString()));
                request.Append(String.Format("<Y>{0}</Y>", ListenerPosition.Position.Y.ToString()));
                request.Append(String.Format("<Z>{0}</Z>", ListenerPosition.Position.Z.ToString()));
                request.Append("</Position>");
                request.Append("<Velocity>");
                request.Append(String.Format("<X>{0}</X>", ListenerPosition.Velocity.X.ToString()));
                request.Append(String.Format("<Y>{0}</Y>", ListenerPosition.Velocity.Y.ToString()));
                request.Append(String.Format("<Z>{0}</Z>", ListenerPosition.Velocity.Z.ToString()));
                request.Append("</Velocity>");
                request.Append("<AtOrientation>");
                request.Append(String.Format("<X>{0}</X>", ListenerPosition.AtOrientation.X.ToString()));
                request.Append(String.Format("<Y>{0}</Y>", ListenerPosition.AtOrientation.Y.ToString()));
                request.Append(String.Format("<Z>{0}</Z>", ListenerPosition.AtOrientation.Z.ToString()));
                request.Append("</AtOrientation>");
                request.Append("<UpOrientation>");
                request.Append(String.Format("<X>{0}</X>", ListenerPosition.UpOrientation.X.ToString()));
                request.Append(String.Format("<Y>{0}</Y>", ListenerPosition.UpOrientation.Y.ToString()));
                request.Append(String.Format("<Z>{0}</Z>", ListenerPosition.UpOrientation.Z.ToString()));
                request.Append("</UpOrientation>");
                request.Append("<LeftOrientation>");
                request.Append(String.Format("<X>{0}</X>", ListenerPosition.LeftOrientation.X.ToString()));
                request.Append(String.Format("<Y>{0}</Y>", ListenerPosition.LeftOrientation.Y.ToString()));
                request.Append(String.Format("<Z>{0}</Z>", ListenerPosition.LeftOrientation.Z.ToString()));
                request.Append("</LeftOrientation>");
                request.Append("</ListenerPosition>");
                request.Append("</Request>");
                request.Append(REQUEST_TERMINATOR);

                _DaemonPipe.SendData(Encoding.ASCII.GetBytes(request.ToString()));

                return _CommandCookie - 1;
            }
            else
            {
                Logger.Log("VoiceManager.RequestStopTuningMode() called when the daemon pipe is disconnected", Helpers.LogLevel.Error, Client);
                return _CommandCookie - 1;
            }
        }

        public int RequestMuteLocalMic(bool enabled)
        {
            if (_DaemonPipe.Connected)
            {
                StringBuilder request = new StringBuilder();
                request.Append(String.Format("<Request requestId=\"{0}\" action=\"Connector.MuteLocalMic.1\">", _CommandCookie++));
                request.Append(String.Format("<ConnectorHandle>{0}</ConnectorHandle>", connectorHandle));
                request.Append(String.Format("<Value>{0}</Value>", enabled.ToString().ToLower()));
                request.Append("</Request>");
                request.Append(REQUEST_TERMINATOR);

                _DaemonPipe.SendData(Encoding.ASCII.GetBytes(request.ToString()));

                return _CommandCookie - 1;
            }
            else
            {
                Logger.Log("VoiceManager.RequestStopTuningMode() called when the daemon pipe is disconnected", Helpers.LogLevel.Error, Client);
                return _CommandCookie - 1;
            }
        }

        public int RequestMuteLocalSpeaker(bool enabled)
        {
            if (_DaemonPipe.Connected)
            {
                StringBuilder request = new StringBuilder();
                request.Append(String.Format("<Request requestId=\"{0}\" action=\"Connector.MuteLocalSpeaker.1\">", _CommandCookie++));
                request.Append(String.Format("<ConnectorHandle>{0}</ConnectorHandle>", connectorHandle));
                request.Append(String.Format("<Value>{0}</Value>", enabled.ToString().ToLower()));
                request.Append("</Request>");
                request.Append(REQUEST_TERMINATOR);

                _DaemonPipe.SendData(Encoding.ASCII.GetBytes(request.ToString()));

                return _CommandCookie - 1;
            }
            else
            {
                Logger.Log("VoiceManager.RequestStopTuningMode() called when the daemon pipe is disconnected", Helpers.LogLevel.Error, Client);
                return _CommandCookie - 1;
            }
        }

        public int RequestSetLocalMicVolume(int volume)
        {
            if (volume < -100 || volume > 100)
                throw new ArgumentException("volume must be between 0 and 100", "volume");

            if (_DaemonPipe.Connected)
            {
                StringBuilder request = new StringBuilder();
                request.Append(String.Format("<Request requestId=\"{0}\" action=\"Connector.SetLocalMicVolume.1\">", _CommandCookie++));
                request.Append(String.Format("<ConnectorHandle>{0}</ConnectorHandle>", connectorHandle));
                request.Append(String.Format("<Value>{0}</Value>", volume.ToString().ToLower()));
                request.Append("</Request>");
                request.Append(REQUEST_TERMINATOR);

                _DaemonPipe.SendData(Encoding.ASCII.GetBytes(request.ToString()));

                return _CommandCookie - 1;
            }
            else
            {
                Logger.Log("VoiceManager.RequestSetSpeakerVolume() called when the daemon pipe is disconnected", Helpers.LogLevel.Error, Client);
                return -1;
            }
        }

        public int RequestSetLocalSpeakerVolume(int volume)
        {
            if (volume < -100 || volume > 100)
                throw new ArgumentException("volume must be between 0 and 100", "volume");

            if (_DaemonPipe.Connected)
            {
                StringBuilder request = new StringBuilder();
                request.Append(String.Format("<Request requestId=\"{0}\" action=\"Connector.SetLocalSpeakerVolume.1\">", _CommandCookie++));
                request.Append(String.Format("<ConnectorHandle>{0}</ConnectorHandle>", connectorHandle));
                request.Append(String.Format("<Value>{0}</Value>", volume.ToString().ToLower()));
                request.Append("</Request>");
                request.Append(REQUEST_TERMINATOR);

                _DaemonPipe.SendData(Encoding.ASCII.GetBytes(request.ToString()));

                return _CommandCookie - 1;
            }
            else
            {
                Logger.Log("VoiceManager.RequestSetSpeakerVolume() called when the daemon pipe is disconnected", Helpers.LogLevel.Error, Client);
                return -1;
            }
        }

        public int RequestSetSpeakerVolume(int volume)
        {
            if (volume < 0 || volume > 100)
                throw new ArgumentException("volume must be between 0 and 100", "volume");

            if (_DaemonPipe.Connected)
            {
                _DaemonPipe.SendData(Encoding.ASCII.GetBytes(String.Format(
                    "<Request requestId=\"{0}\" action=\"Aux.SetSpeakerLevel.1\"><Level>{1}</Level></Request>{2}",
                    _CommandCookie, volume, REQUEST_TERMINATOR)));

                return _CommandCookie - 1;
            }
            else
            {
                Logger.Log("VoiceManager.RequestSetSpeakerVolume() called when the daemon pipe is disconnected", Helpers.LogLevel.Error, Client);
                return -1;
            }
        }

        public int RequestSetCaptureVolume(int volume)
        {
            if (volume < 0 || volume > 100)
                throw new ArgumentException("volume must be between 0 and 100", "volume");

            if (_DaemonPipe.Connected)
            {
                _DaemonPipe.SendData(Encoding.ASCII.GetBytes(String.Format(
                    "<Request requestId=\"{0}\" action=\"Aux.SetMicLevel.1\"><Level>{1}</Level></Request>{2}",
                    _CommandCookie, volume, REQUEST_TERMINATOR)));

                return _CommandCookie - 1;
            }
            else
            {
                Logger.Log("VoiceManager.RequestSetCaptureVolume() called when the daemon pipe is disconnected", Helpers.LogLevel.Error, Client);
                return -1;
            }
        }

        /// <summary>
        /// Does not appear to be working
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="loop"></param>
        public int RequestRenderAudioStart(string fileName, bool loop)
        {
            if (_DaemonPipe.Connected)
            {
                _TuningSoundFile = fileName;

                _DaemonPipe.SendData(Encoding.ASCII.GetBytes(String.Format(
                    "<Request requestId=\"{0}\" action=\"Aux.RenderAudioStart.1\"><SoundFilePath>{1}</SoundFilePath><Loop>{2}</Loop></Request>{3}",
                    _CommandCookie++, _TuningSoundFile, (loop ? "1" : "0"), REQUEST_TERMINATOR)));

                return _CommandCookie - 1;
            }
            else
            {
                Logger.Log("VoiceManager.RequestRenderAudioStart() called when the daemon pipe is disconnected", Helpers.LogLevel.Error, Client);
                return -1;
            }
        }

        public int RequestRenderAudioStop()
        {
            if (_DaemonPipe.Connected)
            {
                _DaemonPipe.SendData(Encoding.ASCII.GetBytes(String.Format(
                    "<Request requestId=\"{0}\" action=\"Aux.RenderAudioStop.1\"><SoundFilePath>{1}</SoundFilePath></Request>{2}",
                    _CommandCookie++, _TuningSoundFile, REQUEST_TERMINATOR)));

                return _CommandCookie - 1;
            }
            else
            {
                Logger.Log("VoiceManager.RequestRenderAudioStop() called when the daemon pipe is disconnected", Helpers.LogLevel.Error, Client);
                return -1;
            }
        }
        #endregion

        private void process_Exited(object sender, EventArgs e)
        {
            if (OnDaemonStoped != null)
                OnDaemonStoped(sender, e);

            process = null;
        }

        private void RequiredVoiceVersionEventHandler(string message, OSD osd, Simulator simulator)
        {
            OSDMap body = (OSDMap)osd;

            if (body.ContainsKey("major_version"))
            {
                int majorVersion = body["major_version"].AsInteger();

                if (VOICE_MAJOR_VERSION != majorVersion)
                {
                    Logger.Log(String.Format("Voice version mismatch! Got {0}, expecting {1}. Disabling the voice manager",
                        majorVersion, VOICE_MAJOR_VERSION), Helpers.LogLevel.Error, Client);
                    Enabled = false;
                }
                else
                {
                    Logger.DebugLog("Voice version " + majorVersion + " verified", Client);
                }
            }
        }

        private void ProvisionCapsResponse(CapsClient client, OSD response, Exception error)
        {
            if (response is OSDMap)
            {
                OSDMap respTable = (OSDMap)response;

                string proxy_server_name = "no data";

                if (OnProvisionAccount != null)
                {
                    try { OnProvisionAccount(respTable["username"].AsString(), respTable["password"].AsString(), respTable["voice_sip_uri_hostname"].AsString(), respTable["voice_account_server_name"].AsString(), proxy_server_name); }
                    catch (Exception e) { Logger.Log(e.Message, Helpers.LogLevel.Error, Client, e); }
                }
            }
        }

        private void ParcelVoiceInfoResponse(CapsClient client, OSD response, Exception error)
        {
            if (response is OSDMap)
            {
                OSDMap respTable = (OSDMap)response;

                string regionName = respTable["region_name"].AsString();
                int localID = (int)respTable["parcel_local_id"].AsInteger();

                string channelURI = null;
                if (respTable["voice_credentials"] is OSDMap)
                {
                    OSDMap creds = (OSDMap)respTable["voice_credentials"];
                    channelURI = creds["channel_uri"].AsString();
                }
                
                if (OnParcelVoiceInfo != null) OnParcelVoiceInfo(regionName, localID, channelURI);
            }
        }

        private void _DaemonPipe_OnDisconnected(SocketException se)
        {
            if (se != null) Console.WriteLine("Disconnected! " + se.Message);
            else Console.WriteLine("Disconnected!");
        }

        private void _DaemonPipe_OnReceiveLine(string line)
        {
            MessagePerse(line);
        }
    }
}
