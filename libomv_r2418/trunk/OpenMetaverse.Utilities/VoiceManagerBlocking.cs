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
using System.Threading;
using OpenMetaverse;

namespace OpenMetaverse.Utilities
{
    public partial class VoiceManager
    {
        /// <summary>Amount of time to wait for the voice daemon to respond.
        /// The value needs to stay relatively high because some of the calls
        /// require the voice daemon to make remote queries before replying</summary>
        public int BlockingTimeout = 30 * 1000;

        protected Dictionary<int, AutoResetEvent> Events = new Dictionary<int, AutoResetEvent>();

        public List<string> CaptureDevices()
        {
            AutoResetEvent evt = new AutoResetEvent(false);
            Events[_CommandCookie] = evt;

            if (RequestCaptureDevices() == -1)
            {
                Events.Remove(_CommandCookie);
                return new List<string>();
            }

            if (evt.WaitOne(BlockingTimeout, false))
                return CurrentCaptureDevices();
            else
                return new List<string>();
        }

        public List<string> RenderDevices()
        {
            AutoResetEvent evt = new AutoResetEvent(false);
            Events[_CommandCookie] = evt;

            if (RequestRenderDevices() == -1)
            {
                Events.Remove(_CommandCookie);
                return new List<string>();
            }

            if (evt.WaitOne(BlockingTimeout, false))
                return CurrentRenderDevices();
            else
                return new List<string>();
        }

        public string CreateConnector(string _accountManagementServer, out int status)
        {
            status = 0;

            AutoResetEvent evt = new AutoResetEvent(false);
            Events[_CommandCookie] = evt;

            if (RequestCreateConnector(_accountManagementServer, string.Empty, false, "3divoice-gateway", ".log", 0) == -1)
            {
                Events.Remove(_CommandCookie);
                return String.Empty;
            }

            bool success = evt.WaitOne(BlockingTimeout, false);
            status = statusCode;

            if (success && statusCode == 200)
                return connectorHandle;
            else
                return String.Empty;
        }

        public string Login(string accountName, string password, string connectorHandle, out int status)
        {
            status = 0;

            AutoResetEvent evt = new AutoResetEvent(false);
            Events[_CommandCookie] = evt;

            if (RequestLogin(accountName, password, string.Empty, connectorHandle, 10) == -1)
            {
                Events.Remove(_CommandCookie);
                return String.Empty;
            }

            bool success = evt.WaitOne(BlockingTimeout, false);
            status = statusCode;

            if (success && statusCode == 200)
                return accountHandle;
            else
                return String.Empty;
        }

        public string SessionCreate(string AccountHandle, string URI, string Name, string Password,
            bool JoinAudio, bool JoinText, string PasswordHashAlgorithm, out int status)
        {
            status = 0;

            AutoResetEvent evt = new AutoResetEvent(false);
            Events[_CommandCookie] = evt;

            if (RequestSessionCreate(accountHandle, URI, Name, Password, JoinAudio, JoinText, PasswordHashAlgorithm, 1) == -1)
            {
                Events.Remove(_CommandCookie);
                return String.Empty;
            }

            bool success = evt.WaitOne(BlockingTimeout, false);
            status = statusCode;

            if (success && statusCode == 200)
                return sessionHandle;
            else
                return String.Empty;
        }

        public void Logout(string accountHandle, out int status)
        {
            status = 0;

            AutoResetEvent evt = new AutoResetEvent(false);
            Events[_CommandCookie] = evt;

            if (RequestLogout(accountHandle) == -1)
            {
                Events.Remove(_CommandCookie);
                return;
            }

            bool success = evt.WaitOne(BlockingTimeout, false);
            status = statusCode;
        }

        public string InitiateShutdown(string connectorHandle, out int status)
        {
            status = 0;

            AutoResetEvent evt = new AutoResetEvent(false);
            Events[_CommandCookie] = evt;

            if (RequestInitiateShutdown(connectorHandle) == -1)
            {
                Events.Remove(_CommandCookie);
                return String.Empty;
            }

            bool success = evt.WaitOne(BlockingTimeout, false);
            status = statusCode;

            if (success && statusCode == 200)
                return connectorHandle;
            else
                return String.Empty;
        }

        protected void RegisterCallbacks()
        {
            OnCaptureDevices += new DevicesCallback(VoiceManager_OnCaptureDevices);
            OnRenderDevices += new DevicesCallback(VoiceManager_OnRenderDevices);
            OnConnectorCreated += new ConnectorCreatedCallback(VoiceManager_OnConnectorCreated);
            OnLogin += new LoginCallback(VoiceManager_OnLogin);
            OnSessionCreated += new SessionCreatedCallback(VoiceManager_OnSessionCreated);
        }

        #region Callbacks

        private void VoiceManager_OnCaptureDevices(int cookie, int statusCode, string statusString, string currentDevice)
        {
            if (Events.ContainsKey(cookie))
                Events[cookie].Set();
        }

        private void VoiceManager_OnRenderDevices(int cookie, int statusCode, string statusString, string currentDevice)
        {
            if (Events.ContainsKey(cookie))
                Events[cookie].Set();
        }

        private void VoiceManager_OnConnectorCreated(int cookie, int statusCode, string versionID, string statusString, string connectorHandle)
        {
            if (Events.ContainsKey(cookie))
                Events[cookie].Set();
        }

        private void VoiceManager_OnLogin(int cookie, int statusCode, string statusString, string accountHandle)
        {
            if (Events.ContainsKey(cookie))
                Events[cookie].Set();
        }

        void VoiceManager_OnSessionCreated(int cookie, int statusCode, string statusString, string sessionHandle)
        {
            if (Events.ContainsKey(cookie))
                Events[cookie].Set();
        }

        #endregion Callbacks
    }
}