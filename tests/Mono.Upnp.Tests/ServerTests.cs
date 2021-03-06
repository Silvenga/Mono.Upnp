// 
// ServerTests.cs
//  
// Author:
//       Scott Peterson <lunchtimemama@gmail.com>
// 
// Copyright (c) 2009 Scott Peterson
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Xml;

using Mono.Ssdp;
using Mono.Upnp.Control;

using NUnit.Framework;

namespace Mono.Upnp.Tests
{
    [Ignore("Ignored due to HttpListener restrictions on Windows.")]
    [TestFixture]
    public class ServerTests
    {
        readonly object mutex = new object ();
        readonly DummyDeserializer deserializer = new DummyDeserializer ();
        
        [Test]
        public void InitialUnicastEventTest ()
        {
            var eventer = new DummyStateVariableEventer ();
            var root = CreateRoot (CreateServiceController (new StateVariable ("Foo", "string", new StateVariableOptions { Eventer = eventer })));
            eventer.SetValue ("foo");
            
            using (var server = new Server (root)) {
                server.Start ();
                var prefix = GeneratePrefix ();
                using (var listener = new HttpListener ()) {
                    listener.Prefixes.Add (prefix);
                    listener.Start ();
                    Exception exception = null;
                    listener.BeginGetContext (result => {
                        try {
                            var context = listener.EndGetContext (result);
                            using (var reader = new StreamReader (context.Request.InputStream)) {
                                Assert.AreEqual (Xml.SingleEventReport, reader.ReadToEnd ());
                            }
                            context.Response.Close ();
                        } catch (Exception e) {
                            exception = e;
                        }
                        lock (mutex) {
                            Monitor.Pulse (mutex);
                        }
                    }, null);
                    
                    Subscribe (root, prefix);
                    
                    if (exception != null) {
                        throw exception;
                    }
                }
            }
        }
        
        [Test]
        public void SingleUpdateUnicastEventTest ()
        {
            var eventer1 = new DummyStateVariableEventer ();
            var eventer2 = new DummyStateVariableEventer ();
            var root = CreateRoot (CreateServiceController (
                new StateVariable ("Foo", "string", new StateVariableOptions { Eventer = eventer1 }),
                new StateVariable ("Bar", "string", new StateVariableOptions { Eventer = eventer2 })
            ));
            eventer1.SetValue ("foo");
            eventer2.SetValue ("bar");
            
            using (var server = new Server (root)) {
                server.Start ();
                var prefix = GeneratePrefix ();
                using (var listener = new HttpListener ()) {
                    listener.Prefixes.Add (prefix);
                    listener.Start ();
                    Exception exception = null;
                    listener.BeginGetContext (result => {
                        try {
                            var context = listener.EndGetContext (result);
                            using (var reader = new StreamReader (context.Request.InputStream)) {
                                Assert.AreEqual (Xml.DoubleEventReport, reader.ReadToEnd ());
                            }
                            context.Response.Close ();
                            listener.BeginGetContext (r => {
                                try {
                                    var c = listener.EndGetContext (r);
                                    using (var reader = new StreamReader (c.Request.InputStream)) {
                                        Assert.AreEqual (Xml.SingleEventReport, reader.ReadToEnd ());
                                    }
                                    c.Response.Close ();
                                } catch (Exception e) {
                                    exception = e;
                                }
                                lock (mutex) {
                                    Monitor.Pulse (mutex);
                                }
                            }, null);
                        } catch (Exception e) {
                            exception = e;
                            lock (mutex) {
                                Monitor.Pulse (mutex);
                            }
                        }
                        eventer1.SetValue ("foo");
                    }, null);
                    
                    Subscribe (root, prefix);
                    
                    if (exception != null) {
                        throw exception;
                    }
                }
            }
        }
        
        [Test]
        public void MultipleUpdateUnicastEventTest ()
        {
            var eventer1 = new DummyStateVariableEventer ();
            var eventer2 = new DummyStateVariableEventer ();
            var root = CreateRoot (CreateServiceController (
                new StateVariable ("Foo", "string", new StateVariableOptions { Eventer = eventer1 }),
                new StateVariable ("Bar", "string", new StateVariableOptions { Eventer = eventer2 })
            ));
            eventer1.SetValue ("foo");
            eventer2.SetValue ("bar");
            
            using (var server = new Server (root)) {
                server.Start ();
                var prefix = GeneratePrefix ();
                using (var listener = new HttpListener ()) {
                    listener.Prefixes.Add (prefix);
                    listener.Start ();
                    Exception exception = null;
                    listener.BeginGetContext (result => {
                        try {
                            var context = listener.EndGetContext (result);
                            using (var reader = new StreamReader (context.Request.InputStream)) {
                                Assert.AreEqual (Xml.DoubleEventReport, reader.ReadToEnd ());
                            }
                            context.Response.Close ();
                            listener.BeginGetContext (resp => {
                                try {
                                    var con = listener.EndGetContext (resp);
                                    using (var reader = new StreamReader (con.Request.InputStream)) {
                                        Assert.AreEqual (Xml.SingleEventReport, reader.ReadToEnd ());
                                    }
                                    con.Response.Close ();
                                    listener.BeginGetContext (r => {
                                        try {
                                            var c = listener.EndGetContext (r);
                                            using (var reader = new StreamReader (c.Request.InputStream)) {
                                                Assert.AreEqual (Xml.DoubleEventReport, reader.ReadToEnd ());
                                            }
                                            c.Response.Close ();
                                        } catch (Exception e) {
                                            exception = e;
                                        }
                                        lock (mutex) {
                                            Monitor.Pulse (mutex);
                                        }
                                    }, null);
                                    eventer1.SetValue ("foo");
                                    eventer2.SetValue ("bar");
                                } catch (Exception e) {
                                    exception = e;
                                    lock (mutex) {
                                        Monitor.Pulse (mutex);
                                    }
                                }
                            }, null);
                            eventer1.SetValue ("foo");
                        } catch (Exception e) {
                            exception = e;
                            lock (mutex) {
                                Monitor.Pulse (mutex);
                            }
                        }
                    }, null);
                    
                    Subscribe (root, prefix);
                    
                    if (exception != null) {
                        throw exception;
                    }
                }
            }
        }
        
        void Subscribe (Root root, string prefix)
        {
            var request = WebRequest.Create (new Uri (root.UrlBase, "/service/0/event/"));
            request.Method = "SUBSCRIBE";
            request.Headers.Add ("CALLBACK", string.Format ("<{0}>", prefix));
            request.Headers.Add ("NT", "upnp:event");
            lock (mutex) {
                using (var response = (HttpWebResponse)request.GetResponse ()) {
                    Assert.AreEqual (HttpStatusCode.OK, response.StatusCode);
                    Assert.IsNotNull (response.Headers["SID"]);
                    Assert.AreEqual ("Second-1800", response.Headers["TIMEOUT"]);
                }
                if (!Monitor.Wait (mutex, TimeSpan.FromSeconds (30))) {
                    Assert.Fail ("Event publishing timed out.");
                }
            }
        }
        
        [Test]
        public void UnsubscribeUnicastEventTest ()
        {
            string sid = null;
            var eventer = new DummyStateVariableEventer ();
            var root = CreateRoot (CreateServiceController (new StateVariable ("Foo", "string", new StateVariableOptions { Eventer = eventer })));
            eventer.SetValue ("foo");
            
            using (var server = new Server (root)) {
                server.Start ();
                var prefix = GeneratePrefix ();
                var url = new Uri (root.UrlBase, "/service/0/event/");
                using (var listener = new HttpListener ()) {
                    listener.Prefixes.Add (prefix);
                    listener.Start ();
                    Exception exception = null;
                    listener.BeginGetContext (result => {
                        lock (mutex) {
                            try {
                                var context = listener.EndGetContext (result);
                                using (var reader = new StreamReader (context.Request.InputStream)) {
                                    Assert.AreEqual (Xml.SingleEventReport, reader.ReadToEnd ());
                                }
                                context.Response.Close ();
                                var unsub_request = WebRequest.Create (url);
                                unsub_request.Method = "UNSUBSCRIBE";
                                unsub_request.Headers.Add ("SID", sid);
                                using (var response = (HttpWebResponse)unsub_request.GetResponse ()) {
                                    Assert.AreEqual (HttpStatusCode.OK, response.StatusCode);
                                }
                                listener.BeginGetContext (r => {
                                    lock (mutex) {
                                        Monitor.Pulse (mutex);
                                    }
                                }, null);
                                eventer.SetValue ("foo");
                            } catch (Exception e) {
                                exception = e;
                                Monitor.Pulse (mutex);
                            }
                        }
                    }, null);
                    var request = WebRequest.Create (url);
                    request.Method = "SUBSCRIBE";
                    request.Headers.Add ("CALLBACK", string.Format ("<{0}>", prefix));
                    request.Headers.Add ("NT", "upnp:event");
                    lock (mutex) {
                        using (var response = (HttpWebResponse)request.GetResponse ()) {
                            Assert.AreEqual (HttpStatusCode.OK, response.StatusCode);
                            Assert.IsNotNull (response.Headers["SID"]);
                            sid = response.Headers["SID"];
                        }
                        if (Monitor.Wait (mutex, TimeSpan.FromSeconds (10))) {
                            Assert.Fail ("The event server sent updates to an unsubscribed client.");
                        }
                    }
                    
                    if (exception != null) {
                        throw exception;
                    }
                }
            }
        }
        
        static ServiceController CreateServiceController ()
        {
            return CreateServiceController (null);
        }
        
        static ServiceController CreateServiceController (StateVariable stateVariable)
        {
            return CreateServiceController (stateVariable, null);
        }
        
        static ServiceController CreateServiceController (StateVariable stateVariable1, StateVariable stateVariable2)
        {
            return new ServiceController (
                new[] {
                    new ServiceAction (
                        "Foo",
                        new[] {
                            new Argument ("bar", "X_ARG_bar", ArgumentDirection.In),
                            new Argument ("result", "X_ARG_result", ArgumentDirection.Out)
                        },
                        arguments => {
                            var out_arguments = new Dictionary<string, string> (1);
                            out_arguments["result"] = string.Format ("You said {0}", arguments["bar"]);
                            return out_arguments;
                        }
                    )
                },
                new[] {
                    new StateVariable ("X_ARG_bar", "string"),
                    new StateVariable ("X_ARG_result", "string"),
                    stateVariable1,
                    stateVariable2
                }
            );
        }
        
        [Test]
        public void ControlTest ()
        {
            var root = CreateRoot (CreateServiceController ());
            
            using (var server = new Server (root)) {
                server.Start ();
                var request = (HttpWebRequest)WebRequest.Create (new Uri (root.UrlBase, "/service/0/control/"));
                request.Method = "POST";
                request.Headers.Add ("SOAPACTION", "urn:schemas-upnp-org:service:mono-upnp-test-service:1#Foo");
                request.ContentType = @"text/xml; charset=""utf-8""";
                var bytes = System.Text.Encoding.UTF8.GetBytes (Xml.SimpleSoapRequest);
                using (var stream = request.GetRequestStream ()) {
                    stream.Write (bytes, 0, bytes.Length);
                }
                using (var response = (HttpWebResponse)request.GetResponse ()) {
                    Assert.AreEqual (HttpStatusCode.OK, response.StatusCode);
                    using (var stream = response.GetResponseStream ()) {
                        using (var reader = XmlReader.Create (stream)) {
                            reader.ReadToFollowing ("result");
                            Assert.AreEqual ("You said hello world!", reader.ReadElementContentAsString ());
                        }
                    }
                }
            }
        }
        
        static Root CreateRoot (ServiceController controller)
        {
            return new DummyRoot (
                new DeviceType ("schemas-upnp-org", "mono-upnp-tests-device", new Version (1, 0)),
                "uuid:d1",
                "Mono.Upnp.Tests Device",
                "Mono Project",
                "Device",
                new DeviceOptions {
                    Services = new[] {
                        new Service (
                            new ServiceType ("schemas-upnp-org", "mono-upnp-test-service", new Version (1, 0)),
                            "urn:upnp-org:serviceId:testService1",
                            controller
                        )
                    }
                }
            );
        }
        
        static Root CreateRoot ()
        {
            return CreateRoot (null, null);
        }
        
        static Root CreateRoot (IEnumerable<Icon> icons1, IEnumerable<Icon> icons2)
        {
            return new DummyRoot (
                new DeviceType ("schemas-upnp-org", "mono-upnp-tests-device", new Version (1, 0)),
                "uuid:d1",
                "Mono.Upnp.Tests Device",
                "Mono Project",
                "Device",
                new DeviceOptions {
                    Icons = icons1,
                    Services = new[] {
                        new DummyService (new ServiceType ("schemas-upnp-org", "mono-upnp-test-service", new Version (1, 0)), "urn:upnp-org:serviceId:testService1"),
                        new DummyService (new ServiceType ("schemas-upnp-org", "mono-upnp-test-service", new Version (2, 0)), "urn:upnp-org:serviceId:testService2"),
                    },
                    EmbeddedDevices = new[] {
                        new Device (
                            new DeviceType ("schemas-upnp-org", "mono-upnp-tests-embedded-device", new Version (1, 0)),
                            "uuid:ed1",
                            "Mono.Upnp.Tests Embedded Device",
                            "Mono Project",
                            "Embedded Device",
                            new DeviceOptions {
                                Icons = icons2,
                                Services = new[] {
                                    new DummyService (new ServiceType ("schemas-upnp-org", "mono-upnp-test-service", new Version (1, 0)), "urn:upnp-org:serviceId:testService1"),
                                    new DummyService (new ServiceType ("schemas-upnp-org", "mono-upnp-test-service", new Version (2, 0)), "urn:upnp-org:serviceId:testService2"),
                                }
                            }
                        )
                    }
                }
            );
        }
        
        [Test]
        public void DescriptionTest ()
        {
            var root = CreateRoot ();
            using (var server = new Server (root)) {
                server.Start ();
                var request = WebRequest.Create (root.UrlBase);
                using (var response = (HttpWebResponse)request.GetResponse ()) {
                    Assert.AreEqual (HttpStatusCode.OK, response.StatusCode);
                    using (var reader = XmlReader.Create (response.GetResponseStream ())) {
                        var target_root = deserializer.DeserializeRoot (reader, root.UrlBase);
                        DeviceDescriptionTests.AssertEquality (root, target_root);
                    }
                }
            }
        }
        
        [Test]
        public void ScpdTest ()
        {
            var controller = CreateServiceController ();
            var root = new DummyRoot (
                new DeviceType ("schemas-upnp-org", "mono-upnp-tests-device", new Version (1, 0)),
                "uuid:d1",
                "Mono.Upnp.Tests Device",
                "Mono Project",
                "Device",
                new DeviceOptions {
                    Services = new[] {
                        new Service (
                            new ServiceType ("uschemas-upnp-org", "mono-upnp-test-service", new Version (1, 0)),
                            "urn:upnp-org:serviceId:testService1",
                            controller
                        )
                    }
                }
            );
            using (var server = new Server (root)) {
                server.Start ();
                var request = WebRequest.Create (new Uri (root.UrlBase, "/service/0/scpd/"));
                using (var response = (HttpWebResponse)request.GetResponse ()) {
                    Assert.AreEqual (HttpStatusCode.OK, response.StatusCode);
                    using (var reader = XmlReader.Create (response.GetResponseStream ())) {
                        var target_controller = deserializer.DeserializeServiceController (reader);
                        ServiceDescriptionTests.AssertEquality (controller, target_controller);
                    }
                }
            }
        }
            
        [Test]
        public void IconTest ()
        {
            var root = CreateRoot (
                new[] {
                    new Icon (100, 100, 32, "image/jpeg", new byte[] { 0 }),
                    new Icon (100, 100, 32, "image/png", new byte[] { 1 })
                },
                new[] {
                    new Icon (100, 100, 32, "image/jpeg", new byte[] { 2 }),
                    new Icon (100, 100, 32, "image/png", new byte[] { 3 })
                }
            );
            using (var server = new Server (root)) {
                server.Start ();
                var url = new Uri (root.UrlBase, "/icon/");
                AssertEquality (url, 0, 0);
                AssertEquality (url, 1, 1);
                url = new Uri (root.UrlBase, "/device/0/icon/");
                AssertEquality (url, 0, 2);
                AssertEquality (url, 1, 3);
            }
        }
                    
        static void AssertEquality (Uri url, int iconIndex, int iconValue)
        {
            var request = WebRequest.Create (new Uri (url, iconIndex.ToString ()));
            using (var response = (HttpWebResponse)request.GetResponse ()) {
                Assert.AreEqual (HttpStatusCode.OK, response.StatusCode);
                using (var stream = response.GetResponseStream ()) {
                    Assert.AreEqual (iconValue, stream.ReadByte ());
                    Assert.AreEqual (-1, stream.ReadByte ());
                }
            }
        }
        
        [Test]
        public void AnnouncementTest ()
        {
            using (var server = new Server (CreateRoot ())) {
                using (var client = new Mono.Ssdp.Client ()) {
                    var announcements = new Dictionary<string,string> ();
                    announcements.Add ("upnp:rootdevice/uuid:d1::upnp:rootdevice", null);
                    announcements.Add ("uuid:d1/uuid:d1", null);
                    announcements.Add ("urn:schemas-upnp-org:device:mono-upnp-tests-device:1/uuid:d1::urn:schemas-upnp-org:device:mono-upnp-tests-device:1", null);
                    announcements.Add ("uuid:ed1/uuid:ed1", null);
                    announcements.Add ("urn:schemas-upnp-org:device:mono-upnp-tests-embedded-device:1/uuid:ed1::urn:schemas-upnp-org:device:mono-upnp-tests-embedded-device:1", null);
                    announcements.Add ("urn:schemas-upnp-org:service:mono-upnp-test-service:1/uuid:d1::urn:schemas-upnp-org:service:mono-upnp-test-service:1", null);
                    announcements.Add ("urn:schemas-upnp-org:service:mono-upnp-test-service:2/uuid:d1::urn:schemas-upnp-org:service:mono-upnp-test-service:2", null);
                    announcements.Add ("urn:schemas-upnp-org:service:mono-upnp-test-service:1/uuid:ed1::urn:schemas-upnp-org:service:mono-upnp-test-service:1", null);
                    announcements.Add ("urn:schemas-upnp-org:service:mono-upnp-test-service:2/uuid:ed1::urn:schemas-upnp-org:service:mono-upnp-test-service:2", null);
                    client.ServiceAdded += (obj, args) => {
                        lock (mutex) {
                            Assert.AreEqual (ServiceOperation.Added, args.Operation);
                            var announcement = string.Format ("{0}/{1}", args.Service.ServiceType, args.Service.Usn);
                            if (announcements.ContainsKey (announcement)) {
                                announcements.Remove (announcement);
                            }
                            if (announcements.Count == 0) {
                                Monitor.Pulse (mutex);
                            }
                        }
                    };
                    client.BrowseAll ();
                    lock (mutex) {
                        server.Start ();
                        if (!Monitor.Wait (mutex, TimeSpan.FromSeconds (30))) {
                            Assert.Fail ("The UPnP server announcement timed out.");
                        }
                    }
                }
            }
        }
        
        static readonly Random random = new Random ();
                                
        static string GeneratePrefix ()
        {
            foreach (var address in Dns.GetHostAddresses (Dns.GetHostName ())) {
                if (address.AddressFamily == AddressFamily.InterNetwork) {
                    return string.Format (
                        "http://{0}:{1}/mono-upnp-tests/event-subscriber/", address, random.Next (1024, 5000));
                }
            }
            
            return null;
        }
    }
}
