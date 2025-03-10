﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Xml;
using Moq;

namespace System.Windows.Forms.Layout.Tests;

// NB: doesn't require thread affinity
public class TableLayoutSettingsTypeConverterTests
{
    public static TheoryData<Type, bool> CanConvertFromData =>
        CommonTestHelper.GetConvertFromTheoryData();

    [Theory]
    [MemberData(nameof(CanConvertFromData))]
    [InlineData(typeof(TableLayoutSettings), false)]
    [InlineData(typeof(string), true)]
    public void TableLayoutSettingsTypeConverter_CanConvertFrom_Invoke_ReturnsExpected(Type sourceType, bool expected)
    {
        TableLayoutSettingsTypeConverter converter = new();
        Assert.Equal(expected, converter.CanConvertFrom(sourceType));
    }

    [Fact]
    public void TableLayoutSettingsTypeConverter_ConvertFrom_HasStylesAndControls_ReturnsExpected()
    {
        TableLayoutSettingsTypeConverter converter = new();
        TableLayoutSettings settings = Assert.IsType<TableLayoutSettings>(converter.ConvertFrom(
@"<?xml version=""1.0"" encoding=""utf-16""?>
<TableLayoutSettings>
    <Controls>
        <Control Name=""simple"" />
        <Control Name=""name"" Row=""1"" RowSpan=""2"" Column=""3"" ColumnSpan=""4"" />
        <Control Name=""invalidRow"" Row=""abc"" />
        <Control Name=""invalidRowSpan"" RowSpan=""abc"" />
        <Control Name=""invalidColumn"" Column=""abc"" />
        <Control Name=""invalidColumnSpan"" ColumnSpan=""abc"" />
    </Controls>
    <Columns Styles=""AutoSize,1,Absolute,2.2"" />
    <Columns Styles=""AutoSize,1.1#2Percent!1"" />
    <Columns Styles=""Percent," + '\u0664' + @""" />
    <Rows Styles=""AutoSize,1,Absolute,2"" />
</TableLayoutSettings>"));
        Assert.Equal(5, settings.ColumnStyles.Count);
        Assert.Equal(SizeType.AutoSize, settings.ColumnStyles[0].SizeType);
        Assert.Equal(1, settings.ColumnStyles[0].Width);
        Assert.Equal(SizeType.Absolute, settings.ColumnStyles[1].SizeType);
        Assert.Equal(2.2f, settings.ColumnStyles[1].Width);
        Assert.Equal(SizeType.AutoSize, settings.ColumnStyles[2].SizeType);
        Assert.Equal(1.12f, settings.ColumnStyles[2].Width);
        Assert.Equal(SizeType.Percent, settings.ColumnStyles[3].SizeType);
        Assert.Equal(1, settings.ColumnStyles[3].Width);
        Assert.Equal(SizeType.Percent, settings.ColumnStyles[4].SizeType);
        Assert.Equal(0, settings.ColumnStyles[4].Width);

        Assert.Equal(2, settings.RowStyles.Count);
        Assert.Equal(SizeType.AutoSize, settings.RowStyles[0].SizeType);
        Assert.Equal(1, settings.RowStyles[0].Height);
        Assert.Equal(SizeType.Absolute, settings.RowStyles[1].SizeType);
        Assert.Equal(2, settings.RowStyles[1].Height);

        Assert.Equal(-1, settings.GetRow("simple"));
        Assert.Equal(1, settings.GetRowSpan("simple"));
        Assert.Equal(-1, settings.GetColumn("simple"));
        Assert.Equal(1, settings.GetColumnSpan("simple"));

        Assert.Equal(1, settings.GetRow("name"));
        Assert.Equal(2, settings.GetRowSpan("name"));
        Assert.Equal(3, settings.GetColumn("name"));
        Assert.Equal(4, settings.GetColumnSpan("name"));

        Assert.Equal(-1, settings.GetRow("invalidRow"));
        Assert.Equal(1, settings.GetRowSpan("invalidRow"));
        Assert.Equal(-1, settings.GetColumn("invalidRow"));
        Assert.Equal(1, settings.GetColumnSpan("invalidRow"));

        Assert.Equal(-1, settings.GetRow("invalidRowSpan"));
        Assert.Equal(1, settings.GetRowSpan("invalidRowSpan"));
        Assert.Equal(-1, settings.GetColumn("invalidRowSpan"));
        Assert.Equal(1, settings.GetColumnSpan("invalidRowSpan"));

        Assert.Equal(-1, settings.GetRow("invalidColumn"));
        Assert.Equal(1, settings.GetRowSpan("invalidColumn"));
        Assert.Equal(-1, settings.GetColumn("invalidColumn"));
        Assert.Equal(1, settings.GetColumnSpan("invalidColumn"));

        Assert.Equal(-1, settings.GetRow("invalidColumnSpan"));
        Assert.Equal(1, settings.GetRowSpan("invalidColumnSpan"));
        Assert.Equal(-1, settings.GetColumn("invalidColumnSpan"));
        Assert.Equal(1, settings.GetColumnSpan("invalidColumnSpan"));
    }

    [Theory]
    [InlineData(@"<?xml version=""1.0"" encoding=""utf-16""?><Root />")]
    [InlineData(@"<?xml version=""1.0"" encoding=""utf-16""?><TableLayoutSettings></TableLayoutSettings>")]
    [InlineData(@"<?xml version=""1.0"" encoding=""utf-16""?><TableLayoutSettings><Controls /><Columns /><Rows /></TableLayoutSettings>")]
    [InlineData(@"<?xml version=""1.0"" encoding=""utf-16""?><TableLayoutSettings><Controls><Control /><Control Name="""" /></Controls><Columns Styles="""" /><Rows Styles="""" /></TableLayoutSettings>")]
    public void TableLayoutSettingsTypeConverter_ConvertFrom_Empty_ReturnsExpected(string value)
    {
        TableLayoutSettingsTypeConverter converter = new();
        TableLayoutSettings settings = Assert.IsType<TableLayoutSettings>(converter.ConvertFrom(value));
        Assert.Empty(settings.ColumnStyles);
        Assert.Empty(settings.RowStyles);
    }

    [Theory]
    [InlineData("")]
    [InlineData("notXmlValue")]
    public void TableLayoutSettingsTypeConverter_ConvertFrom_InvalidStringXmlValue_ThrowsInvalidXmlException(string value)
    {
        TableLayoutSettingsTypeConverter converter = new();
        Assert.Throws<XmlException>(() => converter.ConvertFrom(value));
    }

    [Theory]
    [InlineData("Invalid,1")]
    [InlineData("1,1")]
    [InlineData(",1")]
    public void TableLayoutSettingsTypeConverter_ConvertFrom_InvalidStyleSize_ThrowsArgumentException(string style)
    {
        TableLayoutSettingsTypeConverter converter = new();
        Assert.Throws<ArgumentException>(() => converter.ConvertFrom($@"<?xml version=""1.0"" encoding=""utf-16""?><Root><Columns Styles=""{style}"" /></Root>"));
    }

    [Theory]
    [InlineData("AutoSize")]
    [InlineData("AutoSize,")]
    [InlineData("AutoSize,Invalid")]
    public void TableLayoutSettingsTypeConverter_ConvertFrom_InvalidStyleWidth_ThrowsIndexOutOfRangeException(string style)
    {
        TableLayoutSettingsTypeConverter converter = new();
        Assert.Throws<IndexOutOfRangeException>(() => converter.ConvertFrom($@"<?xml version=""1.0"" encoding=""utf-16""?><Root><Columns Styles=""{style}"" /></Root>"));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(null)]
    public void TableLayoutSettingsTypeConverter_ConvertFrom_InvalidValue_ThrowsNotSupportedException(object value)
    {
        TableLayoutSettingsTypeConverter converter = new();
        Assert.Throws<NotSupportedException>(() => converter.ConvertFrom(value));
    }

    [Theory]
    [InlineData(typeof(string), true)]
    [InlineData(typeof(InstanceDescriptor), true)]
    [InlineData(typeof(TableLayoutSettings), false)]
    [InlineData(typeof(int), false)]
    [InlineData(null, false)]
    public void TableLayoutSettingsTypeConverter_CanConvertTo_Invoke_ReturnsExpected(Type destinationType, bool expected)
    {
        TableLayoutSettingsTypeConverter converter = new();
        Assert.Equal(expected, converter.CanConvertTo(destinationType));
    }

    [WinFormsFact]
    public void TableLayoutSettingsTypeConverter_ConvertTo_NoStylesOrControls_ReturnsExpected()
    {
        using ToolStrip toolStrip = new() { LayoutStyle = ToolStripLayoutStyle.Table };
        TableLayoutSettings settings = Assert.IsType<TableLayoutSettings>(toolStrip.LayoutSettings);
        TableLayoutSettingsTypeConverter converter = new();
        string result = Assert.IsType<string>(converter.ConvertTo(settings, typeof(string)));
        Assert.Equal(@"<?xml version=""1.0"" encoding=""utf-16""?><TableLayoutSettings><Controls /><Columns Styles="""" /><Rows Styles="""" /></TableLayoutSettings>", result);
    }

    [WinFormsFact]
    public void TableLayoutSettingsTypeConverter_ConvertTo_HasStylesAndControls_ReturnsExpected()
    {
        using ToolStrip toolStrip = new() { LayoutStyle = ToolStripLayoutStyle.Table };
        toolStrip.Items.Add(new ToolStripLabel("text"));

        TableLayoutSettings settings = Assert.IsType<TableLayoutSettings>(toolStrip.LayoutSettings);
        settings.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize, 1));
        settings.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 2));
        settings.RowStyles.Add(new RowStyle(SizeType.AutoSize, 1));
        settings.RowStyles.Add(new RowStyle(SizeType.Absolute, 2));

        using ScrollableControl control = new();
        settings.SetColumnSpan(control, 1);
        settings.SetRowSpan(control, 2);
        settings.SetColumn(control, 3);
        settings.SetRow(control, 4);

        TableLayoutSettingsTypeConverter converter = new();
        string result = Assert.IsType<string>(converter.ConvertTo(settings, typeof(string)));
        Assert.Equal(@"<?xml version=""1.0"" encoding=""utf-16""?><TableLayoutSettings><Controls /><Columns Styles=""AutoSize,1,Absolute,2"" /><Rows Styles=""AutoSize,1,Absolute,2"" /></TableLayoutSettings>", result);
    }

    [WinFormsFact]
    public void TableLayoutSettingsTypeConverter_ConvertTo_HasControlChildren_ReturnsExpected()
    {
        using TableLayoutPanel panel = new();
        using ScrollableControl control = new();
        TableLayoutSettings settings = Assert.IsType<TableLayoutSettings>(panel.LayoutSettings);

        panel.Controls.Add(control);
        settings.SetColumnSpan(control, 1);
        settings.SetRowSpan(control, 2);
        settings.SetColumn(control, 3);
        settings.SetRow(control, 4);

        TableLayoutSettingsTypeConverter converter = new();
        string result = Assert.IsType<string>(converter.ConvertTo(settings, typeof(string)));
        Assert.Equal(@"<?xml version=""1.0"" encoding=""utf-16""?><TableLayoutSettings><Controls><Control Name="""" Row=""4"" RowSpan=""2"" Column=""3"" ColumnSpan=""1"" /></Controls><Columns Styles="""" /><Rows Styles="""" /></TableLayoutSettings>", result);
    }

    [Fact]
    public void TableLayoutSettingsTypeConverter_ConvertTo_StubWithoutChildren_ReturnsExpected()
    {
        TableLayoutSettingsTypeConverter converter = new();
        TableLayoutSettings settings = Assert.IsType<TableLayoutSettings>(converter.ConvertFrom(@"<?xml version=""1.0"" encoding=""utf-16""?><Root />"));
        string result = Assert.IsType<string>(converter.ConvertTo(settings, typeof(string)));
        Assert.Equal(@"<?xml version=""1.0"" encoding=""utf-16""?><TableLayoutSettings><Controls /><Columns Styles="""" /><Rows Styles="""" /></TableLayoutSettings>", result);
    }

    [Fact]
    public void TableLayoutSettingsTypeConverter_ConvertTo_StubWithChildren_ReturnsExpected()
    {
        TableLayoutSettingsTypeConverter converter = new();
        TableLayoutSettings settings = Assert.IsType<TableLayoutSettings>(converter.ConvertFrom(@"<?xml version=""1.0"" encoding=""utf-16""?><Root />"));
        settings.SetColumnSpan("name", 1);
        settings.SetRowSpan("name", 2);
        settings.SetColumn("name", 3);
        settings.SetRow("name", 4);

        string result = Assert.IsType<string>(converter.ConvertTo(settings, typeof(string)));
        Assert.Equal(@"<?xml version=""1.0"" encoding=""utf-16""?><TableLayoutSettings><Controls><Control Name=""name"" Row=""4"" RowSpan=""2"" Column=""3"" ColumnSpan=""1"" /></Controls><Columns Styles="""" /><Rows Styles="""" /></TableLayoutSettings>", result);
    }

    [WinFormsFact]
    public void TableLayoutSettingsTypeConverter_ConvertTo_HasControlChildWithoutNameProperty_ThrowsInvalidOperationException()
    {
        using TableLayoutPanel control = new();
        var settings = Assert.IsType<TableLayoutSettings>(control.LayoutSettings);

        using ControlWithNullName child = new();
        control.Controls.Add(child);
        TableLayoutSettingsTypeConverter converter = new();
        Assert.Throws<InvalidOperationException>(() => converter.ConvertTo(settings, typeof(string)));
    }

    [Fact]
    public void TableLayoutSettingsTypeConverter_ConvertTo_HasControlChildWithoutNameStub_ReturnsExpected()
    {
        TableLayoutSettingsTypeConverter converter = new();
        TableLayoutSettings settings = Assert.IsType<TableLayoutSettings>(converter.ConvertFrom(@"<?xml version=""1.0"" encoding=""utf-16""?><Root />"));
        Assert.Throws<ArgumentNullException>("control", () => settings.SetColumnSpan(null, 1));
        string result = Assert.IsType<string>(converter.ConvertTo(settings, typeof(string)));
        Assert.Equal(@"<?xml version=""1.0"" encoding=""utf-16""?><TableLayoutSettings><Controls /><Columns Styles="""" /><Rows Styles="""" /></TableLayoutSettings>", result);
    }

    [WinFormsFact]
    public void TableLayoutSettingsTypeConverter_ConvertTo_RowStyleInColumnStyles_ThrowsInvalidCastException()
    {
        using ToolStrip toolStrip = new() { LayoutStyle = ToolStripLayoutStyle.Table };
        TableLayoutSettings settings = Assert.IsType<TableLayoutSettings>(toolStrip.LayoutSettings);
        Assert.Throws<InvalidCastException>(() => settings.ColumnStyles.Add(new RowStyle()));

        TableLayoutSettingsTypeConverter converter = new();
        Assert.Throws<InvalidCastException>(() => converter.ConvertTo(settings, typeof(string)));
    }

    [WinFormsFact]
    public void TableLayoutSettingsTypeConverter_ConvertTo_ColumnStyleInRowStyles_ThrowsInvalidCastException()
    {
        using ToolStrip toolStrip = new() { LayoutStyle = ToolStripLayoutStyle.Table };
        TableLayoutSettings settings = Assert.IsType<TableLayoutSettings>(toolStrip.LayoutSettings);
        settings.RowStyles.Add(new ColumnStyle());

        TableLayoutSettingsTypeConverter converter = new();
        Assert.Throws<InvalidCastException>(() => converter.ConvertTo(settings, typeof(string)));
    }

    [Fact]
    public void TableLayoutSettingsTypeConverter_ConvertTo_ValueNotTableLayoutSettings_ReturnsExpected()
    {
        TableLayoutSettingsTypeConverter converter = new();
        Assert.Equal("1", converter.ConvertTo(1, typeof(string)));
    }

    [Fact]
    public void TableLayoutSettingsTypeConverter_ConvertTo_NullDestinationType_ThrowsArgumentNullException()
    {
        TableLayoutSettingsTypeConverter converter = new();
        Assert.Throws<ArgumentNullException>("destinationType", () => converter.ConvertTo(new object(), null));
    }

    [Theory]
    [InlineData(typeof(InstanceDescriptor))]
    [InlineData(typeof(TableLayoutSettings))]
    [InlineData(typeof(int))]
    public void TableLayoutSettingsTypeConverter_ConvertTo_InvalidDestinationType_ThrowsNotSupportedException(Type destinationType)
    {
        TableLayoutSettingsTypeConverter converter = new();
        using ToolStrip toolStrip = new() { LayoutStyle = ToolStripLayoutStyle.Table };
        Assert.Throws<NotSupportedException>(() => converter.ConvertTo(toolStrip.LayoutSettings, destinationType));
    }

    [TypeDescriptionProvider(typeof(CustomTypeDescriptionProvider))]
    private class ControlWithNullName : Control
    {
    }

    private class CustomTypeDescriptionProvider : TypeDescriptionProvider
    {
        public CustomTypeDescriptionProvider()
        {
        }

        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
        {
            Mock<ICustomTypeDescriptor> mockDescriptor = new(MockBehavior.Strict);
            mockDescriptor
                .Setup(c => c.GetProperties())
                .Returns(new PropertyDescriptorCollection(Array.Empty<PropertyDescriptor>()));
            return mockDescriptor.Object;
        }
    }
}
