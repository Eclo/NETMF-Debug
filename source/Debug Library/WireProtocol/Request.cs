//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Microsoft .NET Micro Framework and is unsupported. 
// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use these files except in compliance with the License.
// You may obtain a copy of the License at:
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing
// permissions and limitations under the License.
// 
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using Microsoft.SPOT.Debugger.WireProtocol;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.SPOT.Debugger
{
    public delegate void CommandEventHandler(IncomingMessage msg, bool fReply);

    internal class Request
    {
        internal Controller ctrl;
        internal OutgoingMessage outgoingMsg;
        internal IncomingMessage responseMsg;
        internal int retries;
        internal TimeSpan waitRetryTimeout;
        internal TimeSpan totalWaitTimeout;
        internal CommandEventHandler callback;
        internal ManualResetEvent m_event;
        internal Timer timer;

        internal Request(Controller ctrl, OutgoingMessage outMsg, int retries, int timeout, CommandEventHandler callback)
        {
            if (retries < 0)
            {
                throw new ArgumentException("Value cannot be negative", "retries");
            }

            if (timeout < 1 || timeout > 60 * 60 * 1000)
            {
                throw new ArgumentException(String.Format("Value out of bounds: {0}", timeout), "timeout");
            }

            //this.parent = parent;
            this.ctrl = ctrl;
            this.outgoingMsg = outMsg;
            this.retries = retries;
            waitRetryTimeout = new TimeSpan(timeout * TimeSpan.TicksPerMillisecond);
            totalWaitTimeout = new TimeSpan((retries == 0 ? 1 : 2 * retries) * timeout * TimeSpan.TicksPerMillisecond);
            this.callback = callback;

            if (callback == null)
                m_event = new ManualResetEvent(false);
        }

        internal bool MatchesReply(IncomingMessage res)
        {
            Packet headerReq = outgoingMsg.Header;
            Packet headerRes = res.Header;

            if (headerReq.m_cmd == headerRes.m_cmd &&
               headerReq.m_seq == headerRes.m_seqReply)
            {
                return true;
            }

            return false;
        }

        internal IncomingMessage Wait()
        {
            if (m_event == null)
                return responseMsg;

            //var waitStartTime = DateTime.UtcNow;
            //var requestTimedOut = !m_event.WaitOne(m_waitRetryTimeout, false);

            //// Wait for m_waitRetryTimeout milliseconds, if we did not get a signal by then
            //// attempt sending the request again, and then wait more.
            //while (requestTimedOut)
            //{
            //    var deltaT = DateTime.UtcNow - waitStartTime;
            //    if (deltaT >= m_totalWaitTimeout)
            //        break;

            //    if (m_retries <= 0)
            //        break;

            //    //if( m_outMsg.Send( ) )
            //    m_retries--;
            //    //m_event.Reset();

            //    requestTimedOut = !m_event.WaitOne(m_waitRetryTimeout, false);
            //}

            //if (requestTimedOut)
            //    m_parent.CancelRequest(this);

            //if (responseMsg == null && m_parent.ThrowOnCommunicationFailure)
            //{
            //    //do we want a separate exception for aborted requests?
            //    throw new IOException("Request failed");
            //}

            return responseMsg;
        }

        internal void Signal(IncomingMessage res)
        {
            lock (this)
            {
                if (timer != null)
                {
                    timer.Dispose();
                    timer = null;
                }

                responseMsg = res;
            }

            Signal();
        }

        internal void Signal()
        {
            CommandEventHandler callback;
            IncomingMessage res;

            lock (this)
            {
                callback = this.callback;
                res = responseMsg;

                if (timer != null)
                {
                    timer.Dispose();
                    timer = null;
                }

                if (m_event != null)
                {
                    m_event.Set();
                }
            }

            if (callback != null)
            {
                callback(res, true);
            }
        }

        internal async Task<IncomingMessage> PerformRequestAsync(CancellationToken cancellationToken)
        {
            int retryCounter = 0;

            IncomingMessage reply;
            var reassembler = new MessageReassembler(ctrl, this);


            while (retryCounter++ < retries)
            {
                // send message
                if (await outgoingMsg.SendAsync().ConfigureAwait(false))
                {
                    // need to have a timeout to cancel the process task otherwise it may end up waiting forever for this to return
                    // because we have an external cancellation token and the above timeout cancellation token, need to combine both
                    reply = await reassembler.ProcessAsync(cancellationToken.AddTimeout(waitRetryTimeout)).ConfigureAwait(false);

                    if (reply != null)
                    {
                        return reply;
                    }
                }
                else
                {
                    // send failed
                    Debug.WriteLine("SEND FAILED...");
                }

                // something went wrong, retry with a progressive back-off strategy
                await Task.Delay(200 * retryCounter);
            }

            return null;
        }
    }
}
