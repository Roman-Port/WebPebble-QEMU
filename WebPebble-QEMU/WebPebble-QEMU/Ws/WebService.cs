using System;
using System.Collections.Generic;
using System.Text;
using WebSocketSharp.Server;
using System.Linq;
using WebSocketSharp;
using Newtonsoft.Json;
using WebPebble_QEMU.Ws.Entities;

namespace WebPebble_QEMU.Ws
{
    public class WebService : WebSocketBehavior
    {
        public EmulatorStatus status = EmulatorStatus.Waiting;
        public QemuSession session;
        public int sessionId = -1;

        protected override void OnMessage(MessageEventArgs e)
        {
            //Parse JSON.
            WebRequest req = JsonConvert.DeserializeObject<WebRequest>(e.Data);
            //Decide what to do
            switch(req.type)
            {
                case WebRequestType.BootNew:
                    OnBootRequest(req);
                    return;
            }
            base.OnMessage(e);
        }

        protected override void OnError(WebSocketSharp.ErrorEventArgs e)
        {
            OnKillEmu();
            base.OnError(e);
        }

        protected override void OnOpen()
        {
            base.OnOpen();
        }

        protected override void OnClose(CloseEventArgs e)
        {
            OnKillEmu();
            base.OnClose(e);
        }

        private void SendData<T>(T data, WebReplyType type)
        {
            var d = new WebReply<T>();
            d.data = data;
            d.type = type;
            Send(JsonConvert.SerializeObject(d));
        }

        /* Inner commands */
        private void ChangeState(EmulatorStatus s)
        {
            //Notify client of this change.
            OnStageChange e = new OnStageChange
            {
                old_status = status,
                new_status = s
            };
            SendData(e, WebReplyType.OnStatusChange);
            status = s;
        }

        private void OnFatalError(string description, Exception ex)
        {
            OnFatalError(description, ex.Message + " at \n" + ex.StackTrace);
        }

        private void OnFatalError(string description, string more)
        {
            //Uh oh. Shut down everything.
            try
            {
                session.EndSession();
            }
            catch
            {

            }
            status = EmulatorStatus.Crashed;
            //Tell the clients something went wrong.
            OnFatalError e = new OnFatalError
            {
                error_text = description,
                error_text_more = more
            };
            SendData(e, WebReplyType.OnFatalError);
        }

        private void OnKillEmu()
        {
            //Kill emulator. Don't tell the client because they probably either disconnected, or sent their own disconnect command.
            if(session != null)
                session.EndSession();
            //Free the slot.
            if (sessionId != -1)
                Program.open_ids[sessionId] = false;
        }

        /* API */
        private void OnBootRequest(WebRequest req)
        {
            //If we're already booted, ignore this. Else, start a new session.
            if (status != EmulatorStatus.Waiting)
                return;
            //We're creating a new session. Get the platform.
            if(!req.data.ContainsKey("platform"))
            {
                //No platform sent.
                OnFatalError("Error Launching Emulator: No Platform", "The required key 'platform' wasn't sent.");
                return;
            } 
            string platform = (string)req.data["platform"];
            if(!Program.config.flash_bins.ContainsKey(platform))
            {
                //Platform invalid.
                OnFatalError("Error Launching Emulator: Bad Platform", "The platform '"+platform+"' wasn't a valid platform on this system.");
                return;
            }
            //Check to see if there is an open ID.
            sessionId = -1;
            for(int i = 1; i<Program.open_ids.Length; i++)
            {
                if (Program.open_ids[i] == false)
                {
                    //Found an open session.
                    Program.open_ids[i] = true;
                    sessionId = i;
                    break;
                }
            }
            if(sessionId == -1)
            {
                //No sessions!
                OnFatalError("Error Launching Emulator: Server Overloaded", "The server is at maximum capacity! Wait a few minutes and try again.");
                return;
            }
            //We're good to go. Let the client know that we're starting.
            ChangeState(EmulatorStatus.Booting);
            session = null;
            try
            {
                session = QemuSession.SpawnSession(sessionId, platform);
            } catch (Exception ex)
            {
                OnFatalError("Error Launching Emulator: Unexpected Error", ex);
            }
            //The emulator has started. Let the client know and provide some extra data.
            status = EmulatorStatus.Idle;
            OnEmulatorBoot b = new OnEmulatorBoot();
            b.vnc_addr = this.Context.Host + ":59" + sessionId.ToString("00");
            SendData(b, WebReplyType.OnEmulatorBoot);
        }
    }

    public enum EmulatorStatus
    {
        Waiting, //Waiting for boot options
        Booting, //Starting
        Idle, //General state
        Crashed, //Died
        Ended //User shut down the emulator.
    }
}
