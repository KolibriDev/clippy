using System;
using System.Linq;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kolibri;

namespace ClippyTest
{
    [TestClass]
    public class BasicUsage
    {
        [TestInitialize]
        public void Setup()
        {
            Clipboard.Clear();
        }

        private static readonly string[] clipboardDataFormats =
        {
            DataFormats.Bitmap,
            DataFormats.CommaSeparatedValue,
            DataFormats.Dib,
            DataFormats.Dif,
            DataFormats.EnhancedMetafile,
            DataFormats.FileDrop,
            DataFormats.Html,
            DataFormats.Locale,
            DataFormats.MetafilePict,
            DataFormats.OemText,
            DataFormats.Palette,
            DataFormats.PenData,
            DataFormats.Riff,
            DataFormats.Rtf,
            DataFormats.Serializable,
            DataFormats.StringFormat,
            DataFormats.SymbolicLink,
            DataFormats.Text,
            DataFormats.Tiff,
            DataFormats.UnicodeText,
            DataFormats.WaveAudio
        };

        private static bool IsClipboardNonEmpty()
        {
            return clipboardDataFormats.Any(Clipboard.ContainsData);
        }

        private static bool IsClipboardEmpty()
        {
            return !IsClipboardNonEmpty();
        }

        [TestMethod]
        public void PushNullStringToClipboardIsntAllowed()
        {
            var result = Clippy.PushStringToClipboard(null);
            Assert.IsNotNull(result);
            Assert.AreEqual(Clippy.ResultCode.ErrorInvalidArgs, result.ResultCode);
            Assert.IsTrue(IsClipboardEmpty());
        }

        [TestMethod]
        public void PushAsciiStringToClipboardWillBeAscii()
        {
            var p = "asdf";
            var result = Clippy.PushStringToClipboard(p);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.OK);
            Assert.IsTrue(Clipboard.ContainsData(DataFormats.Text));
            Assert.IsTrue(Clipboard.GetText() == p);
        }

        [TestMethod]
        public void PushNonAsciiStringToClipboardWillBeUnicode()
        {
            var p = "áéíóúýðæö";
            var result = Clippy.PushStringToClipboard(p);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.OK);
            Assert.IsTrue(Clipboard.ContainsData(DataFormats.UnicodeText));
            Assert.IsTrue(Clipboard.GetText() == p);
        }

        [TestMethod]
        public void PushEmptyStringToClipboardDoesntBomb()
        {
            const string p = "";
            var result = Clippy.PushStringToClipboard(p);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.OK);
            Assert.IsTrue(Clipboard.ContainsData(DataFormats.UnicodeText));
            Assert.IsTrue(Clipboard.GetText() == p);
        }

        [TestMethod]
        public void PushAnyStringToClipboardDoesntBomb()
        {
            const string p = "Clippy!";
            var result = Clippy.PushStringToClipboard(p);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.OK);
            Assert.IsFalse(IsClipboardEmpty());
            Assert.IsTrue(Clipboard.ContainsData(DataFormats.UnicodeText));
            Assert.IsTrue(Clipboard.GetText() == p);
        }

        private enum WindowsErrorCodes : uint
        {
            Success = 0,
            ErrorNotEnoughMemory = 8
        }

        [TestMethod]
        public void PushHugeStringToClipboardDoesntBomb()
        {
            var stuff = "1234567890";
            for (var i = 0; i < 99; ++i)
            {
                var outOfMemory = false;
                try
                {
                    stuff += stuff;
                }
                catch (OutOfMemoryException)
                {
                    outOfMemory = true;
                }

                var result = Clippy.PushStringToClipboard(stuff);
                Assert.IsNotNull(result);
                switch (result.ResultCode)
                {
                    case Clippy.ResultCode.Success:
                        break;
                    case Clippy.ResultCode.ErrorGlobalAlloc:
                        Assert.AreEqual(WindowsErrorCodes.ErrorNotEnoughMemory, (WindowsErrorCodes)result.LastError);
                        Assert.IsFalse(Clipboard.GetText() == stuff);
                        break;
                    case Clippy.ResultCode.ErrorOutOfMemoryException:
                        Assert.AreEqual(WindowsErrorCodes.Success, (WindowsErrorCodes)result.LastError);
                        Assert.IsFalse(Clipboard.GetText() == stuff);
                        break;
                    default:
                        Assert.Fail("Unexpected result code ({0}) and windows error ({1})", result.ResultCode, result.LastError);
                        break;
                }

                if (outOfMemory)
                {
                    Assert.IsFalse(result.OK);
                }
                else
                {
                    if (result.OK)
                    {
                        Assert.IsTrue(Clipboard.ContainsData(DataFormats.UnicodeText));
                        Assert.IsTrue(Clipboard.GetText() == stuff);
                    }
                    else
                    {
                        Assert.IsFalse(Clipboard.GetText() == stuff);
                        return;
                    }
                }
            }
            Assert.Inconclusive();
        }
    }
}
