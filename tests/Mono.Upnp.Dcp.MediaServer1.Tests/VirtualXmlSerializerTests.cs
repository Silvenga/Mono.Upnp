// 
// VirtualXmlSerializerTests.cs
//  
// Author:
//       Scott Thomas <lunchtimemama@gmail.com>
// 
// Copyright (c) 2010 Scott Thomas
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

using NUnit.Framework;

using Mono.Upnp.Dcp.MediaServer1.Xml;
using Mono.Upnp.Xml;

namespace Mono.Upnp.Dcp.MediaServer1.Tests
{
    //[TestFixture]
    //public class VirtualXmlSerializerTests
    //{
    //    XmlSerializer<VirtualContext> xml_serializer = new XmlSerializer<VirtualContext> (
    //        (serializer, type) => new VirtualDelegateSerializationCompiler (serializer, type));

    //    [XmlType ("element")]
    //    class Element<T>
    //    {
    //        [XmlElement ("foo")] public T Foo { get; set; }
    //    }

    //    [Test]
    //    public void Element ()
    //    {
    //        var data = new Element<string> { Foo = "bar" };
    //        AssertAreEqual ("<element><foo>foo</foo></element>", data, new Override ("foo", "foo"));
    //    }

    //    [Test]
    //    public void ElementWithoutMatchingOverrides ()
    //    {
    //        var data = new Element<string> { Foo = "bar" };
    //        AssertAreEqual ("<element><foo>bar</foo></element>", data,
    //            new Override ("bat", "bat"), new Override ("baz", "baz"));
    //    }

    //    [Test]
    //    public void ElementWithMatchingAndNonMatchingOverrides ()
    //    {
    //        var data = new Element<string> { Foo = "bar" };
    //        AssertAreEqual ("<element><foo>foo</foo></element>", data,
    //            new Override ("bat", "bat"), new Override ("foo", "foo"));
    //    }

    //    [Test]
    //    public void UnmatchingNamespacedElement ()
    //    {
    //        var data = new Element<string> { Foo = "bar" };
    //        AssertAreEqual ("<element><foo>bar</foo></element>", data, new Override ("foo", "foo", "http://test"));
    //    }

    //    class ElementOverride<T> : IXmlSerializable<VirtualContext>
    //    {
    //        Element<T> element;

    //        public ElementOverride (Element<T> element)
    //        {
    //            this.element = element;
    //        }

    //        public void Serialize (XmlSerializationContext<VirtualContext> context)
    //        {
    //            context.AutoSerializeObjectStart (element);
    //            SerializeMembers (context);
    //            context.AutoSerializeObjectEnd (element);
    //        }

    //        public void SerializeMembers (XmlSerializationContext<VirtualContext> context)
    //        {
    //            context.AutoSerializeMembers (element, new VirtualContext (new Override ("bat", "foo")));
    //        }
    //    }

    //    [Test]
    //    public void ElementOverride ()
    //    {
    //        var element_override = new ElementOverride<string> (new Element<string> { Foo = "foo" });
    //        AssertAreEqual ("<element><foo>bat</foo></element>", element_override);
    //    }

    //    [XmlType ("element")]
    //    class ElementOmitIfNull
    //    {
    //        [XmlElement ("foo", OmitIfNull = true)] public string Foo { get; set; }
    //    }

    //    [Test]
    //    public void OverrideElementWithNull ()
    //    {
    //        var element = new ElementOmitIfNull { Foo = "bar" };
    //        AssertAreEqual ("<element />", element, new Override (null, "foo"));
    //    }

    //    [Test]
    //    public void OverrideElementWithNonNull ()
    //    {
    //        var element = new ElementOmitIfNull ();
    //        AssertAreEqual ("<element><foo>bar</foo></element>", element, new Override ("bar", "foo"));
    //    }

    //    [Test]
    //    public void IntElement ()
    //    {
    //        var element = new Element<int> { Foo = 42 };
    //        AssertAreEqual ("<element><foo>13</foo></element>", element, new Override (13, "foo"));
    //    }

    //    [XmlType ("element")]
    //    class NamespacedElement<T>
    //    {
    //        [XmlElement ("foo", "http://test")] public T Foo { get; set; }
    //    }

    //    [Test]
    //    public void NamespacedElement ()
    //    {
    //        var element = new NamespacedElement<string> { Foo = "bar" };
    //        AssertAreEqual (@"<element><foo xmlns=""http://test"">foo</foo></element>", element,
    //            new Override ("foo", "foo", "http://test"));
    //    }

    //    [Test]
    //    public void UnmatchingUnnamespacedElement ()
    //    {
    //        var element = new NamespacedElement<string> { Foo = "bar" };
    //        AssertAreEqual (@"<element><foo xmlns=""http://test"">bar</foo></element>", element,
    //            new Override ("foo", "foo"));
    //    }

    //    [XmlType ("attribute")]
    //    class Attribute<T>
    //    {
    //        [XmlAttribute ("foo")] public T Foo { get; set; }
    //    }

    //    [Test]
    //    public void Attribute ()
    //    {
    //        var attribute = new Attribute<string> { Foo = "bar" };
    //        AssertAreEqual (@"<attribute foo=""foo"" />", attribute, new Override ("foo", "foo"));
    //    }

    //    [Test]
    //    public void UnmatchingNamespacedAttribute ()
    //    {
    //        var attribute = new Attribute<string> { Foo = "bar" };
    //        AssertAreEqual (@"<attribute foo=""bar"" />", attribute, new Override ("foo", "foo", "http://test"));
    //    }

    //    [XmlType ("attribute")]
    //    class NamespacedAttribute<T>
    //    {
    //        [XmlAttribute ("foo", "http://test", "t")] public T Foo { get; set; }
    //    }

    //    [Test]
    //    public void NamespacedAttribute ()
    //    {
    //        var attribute = new NamespacedAttribute<string> { Foo = "bar" };
    //        AssertAreEqual (@"<attribute t:foo=""foo"" xmlns:t=""http://test"" />", attribute,
    //            new Override ("foo", "foo", "http://test"));
    //    }

    //    [Test]
    //    public void UnmatchingWronglyNamespacedAttribute ()
    //    {
    //        var attribute = new NamespacedAttribute<string> { Foo = "bar" };
    //        AssertAreEqual (@"<attribute t:foo=""bar"" xmlns:t=""http://test"" />", attribute,
    //            new Override ("foo", "foo", "http://fail"));
    //    }

    //    void AssertAreEqual<T> (string xml, T obj, params Override[] overrides)
    //    {
    //        Assert.AreEqual (xml, xml_serializer.GetString (obj, new XmlSerializationOptions<VirtualContext> {
    //            Context = new VirtualContext (overrides),
    //            XmlDeclarationType = XmlDeclarationType.None
    //        }));
    //    }
    //}
}
