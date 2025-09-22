using Microsoft.VisualStudio.TestTools.UnitTesting;
using Core.FileSystem;
using System.Collections.Generic;

namespace Testing
{
    [TestClass]
    public class NullDelimitedAppendedJsonStringsFileHelperTests
    {

        [TestInitialize]
        public void Initialize()
        {
            //_UserRoutingTable = new UserRoutingTable(nodes, webSocketServer);
        }

        [TestMethod]
        public void TestAppendCompletes()
        {
            using (TemporaryFile temporaryFile = new TemporaryFile(".bin"))
            {
                string[] strings = new string[] { "{\"name\":\"a\",\"value\":1}", "{\"name\":\"a\",\"value\":2}" };
                foreach (string str in strings)
                    NullDelimitedAppendedJsonStringsFileHelper.Append(temporaryFile.FilePath, str);
            }
        }

        [TestMethod]
        public void TestReadBack()
        {
            using (TemporaryFile temporaryFile = new TemporaryFile(".bin"))
            {
                string[] strings = new string[] { "{\"name\":\"a\",\"value\":3}", "{\"name\":\"a\",\"value\":4}" };
                foreach (string str in strings)
                    NullDelimitedAppendedJsonStringsFileHelper.Append(temporaryFile.FilePath, str);
                List<string> jsonStrings = new List<string>();
                NullDelimitedAppendedJsonStringsFileHelper.Read(temporaryFile.FilePath, jsonStrings, 2, out long startIndexFromBeginningInclusive);

            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            //_TicketedSender.Dispose();
        }
    }
}
