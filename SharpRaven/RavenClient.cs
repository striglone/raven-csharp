using System;
using SharpRaven.Data;
using System.Net;
using System.IO;
using SharpRaven.Utilities;
using System.Web;

namespace SharpRaven {
    public class RavenClient {

        public DSN CurrentDSN { get; set; }

        /// <summary>
        /// Enable Gzip Compression?
        /// Defaults to true.
        /// </summary>
        public bool Compression { get; set; }

        public RavenClient(string dsn) {
            CurrentDSN = new DSN(dsn);
            Compression = true;
        }

        public RavenClient(DSN dsn) {
            CurrentDSN = dsn;
            Compression = true;
        }

        public int CaptureException(Exception e)
        {
            JsonPacket packet = new JsonPacket(CurrentDSN.ProjectID, e);
            Send(packet, CurrentDSN);
            return 0;
        }

        public int CaptureException(Exception e, string[] tags)
        {
            return 0;
        }

        public int CaptureMessage(string message, ErrorLevel level = ErrorLevel.info, string[] tags = null)
        {
            JsonPacket packet = new JsonPacket(CurrentDSN.ProjectID);
            packet.Message = message;
            packet.Level = level;

            Send(packet, CurrentDSN);

            return 0;
        }

        public bool Send(JsonPacket jp, DSN dsn) {
            try {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(dsn.SentryURI);
                request.Method = "POST";
                request.Accept = "application/json";
                request.ContentType = "application/json; charset=utf-8";
                request.Headers.Add("X-Sentry-Auth", PacketBuilder.CreateAuthenticationHeader(dsn));
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
                
                request.UserAgent = "RavenSharp/1.0";

                Console.WriteLine("Header: " + PacketBuilder.CreateAuthenticationHeader(dsn));
                Console.WriteLine("Packet: " + jp.Serialize());

                // Write the messagebody.
                using (Stream s = request.GetRequestStream()) {
                    using (StreamWriter sw = new StreamWriter(s)) {
                        // Compress and encode.
                        //string data = Utilities.GzipUtil.CompressEncode(jp.Serialize());
                        //Console.WriteLine("Writing: " + data);
                        // Write to the JSON script when ready.
                        sw.Write(jp.Serialize());
                        // Close streams.
                        sw.Flush();
                        sw.Close();
                    }
                    s.Flush();
                    s.Close();
                }

                HttpWebResponse wr = (HttpWebResponse)request.GetResponse();
            } catch (WebException e) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("[ERROR] ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(e.Message);

                string messageBody = String.Empty;
                using (StreamReader sw = new StreamReader(e.Response.GetResponseStream())) {
                    messageBody = sw.ReadToEnd();
                }
                Console.WriteLine("[MESSAGE BODY] " + messageBody);
                
                return false;
            }

            return true;
        }

        #region Deprecated methods

        /**
         *  These methods have been deprectaed in favour of the ones
         *  that have the same names as the other sentry clients, this
         *  is purely for the sake of consistency
         */
         
        [Obsolete("The more common CaptureException method should be used")]
        public int CaptureEvent(Exception e)
        {
            return this.CaptureException(e);
        }

        [Obsolete("The more common CaptureException method should be used")]
        public int CaptureEvent(Exception e, string[] tags)
        {
            return this.CaptureException(e, tags);
        }

        #endregion
    }
}
