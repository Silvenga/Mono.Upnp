// 
// MainWindow.cs
//  
// Author:
//       Scott Thomas <lunchtimemama@gmail.com>
// 
// Copyright (c) 2009 Scott Thomas
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

using Gtk;

using Mono.Addins;
using Mono.Unix;

namespace Mono.Upnp.GtkClient
{
    public partial class MainWindow : Gtk.Window
    {
        readonly Client client;
        readonly ListStore model;
        bool connected;
        
        public MainWindow () : base (Gtk.WindowType.Toplevel)
        {
            Build ();
            
            list.AppendColumn (Catalog.GetString ("UPnP Devices"), new CellRendererText (), RenderCell);
            
            model = new ListStore (typeof (DeviceAnnouncement));
            list.Model = model;
            
            client = new Client ();
            client.DeviceAdded += ClientDeviceAdded;
            client.ServiceAdded += ClientServiceAdded;
        }

        void ClientServiceAdded (object sender, ServiceEventArgs e)
        {
            model.AppendValues (e.Service);
        }
        
        void RenderCell (TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter)
        {
            var value = model.GetValue (iter, 0);
            
            if (value == null) return;
            
            var service = value as ServiceAnnouncement;
            if (service != null) {
                ((CellRendererText)cell).Text = string.Format ("{0} - {1}", service.Type, service.DeviceUdn);
            } else {
                ((CellRendererText)cell).Text = ((DeviceAnnouncement)value).Udn;
            }
        }

        void ClientDeviceAdded (object sender, DeviceEventArgs e)
        {
            model.AppendValues (e.Device);
        }
    
        protected void OnDeleteEvent (object sender, Gtk.DeleteEventArgs a)
        {
            Gtk.Application.Quit ();
            a.RetVal = true;
        }

        protected virtual void OnConnectActionToggled (object sender, System.EventArgs e)
        {
            if (connected) {
                Disconnect ();
            } else {
                Connect ();
            }
        }
        
        void Connect ()
        {
            connected = true;
            connectAction.IconName = "gtk-disconnect";
            connectAction.Label = Catalog.GetString ("Disconnect");
            client.BrowseAll ();
        }
        
        void Disconnect ()
        {
            connected = false;
            connectAction.IconName = "gtk-connect";
            connectAction.Label = Catalog.GetString ("Connect");
            //client.Stop ();
        }

        protected virtual void OnListRowActivated (object o, Gtk.RowActivatedArgs args)
        {
            TreeIter iter;
            if (!model.GetIter (out iter, args.Path)) {
                return;
            }
            
            infoBox.Remove (infoBox.Children[0]);
            
            var value = model.GetValue (iter, 0);
            var service = value as ServiceAnnouncement;
            if (service != null) {
                infoBox.Add (CreateNotebook (service));
            } else {
                infoBox.Add (CreateNotebook ((DeviceAnnouncement)value));
            }
            infoBox.ShowAll ();
        }
        
        Widget CreateNotebook (DeviceAnnouncement device)
        {
            var notebook = new Notebook ();
            foreach (var provider in DeviceInfoProviders) {
                if (provider.CanProvideInfo (device.Type)) {
                    notebook.AppendPage (new LazyDeviceInfo (provider, device), new Label (provider.Name));
                }
            }
            
            return notebook;
        }
        
        Widget CreateNotebook (ServiceAnnouncement service)
        {
            var notebook = new Notebook ();
            foreach (var provider in ServiceInfoProviders) {
                if (provider.CanProvideInfo (service.Type)) {
                    notebook.AppendPage (new LazyServiceInfo (provider, service), new Label (provider.Name));
                }
            }
            return notebook;
        }
        
        static IEnumerable<IDeviceInfoProvider> DeviceInfoProviders {
            get {
                yield return new DeviceAnnouncementInfoProvider ();
                yield return new DeviceDescriptionInfoProvider ();
            }
        }
        
        static IEnumerable<IServiceInfoProvider> ServiceInfoProviders {
            get {
                yield return new ServiceAnnouncementInfoProvider ();
                yield return new ServiceDescriptionInfoProvider ();
            }
        }
    }
}
