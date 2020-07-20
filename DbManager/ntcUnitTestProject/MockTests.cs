using DbManager.Logic;
using DbManager.Logic.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ntcUnitTestProject
{
    [TestClass]
    public class MockTests
    {
        [TestMethod]
        public void WhenChecksumAlreadyExists_ReturnTrue()
        {
            Mock<IMetaData> mock = new Mock<IMetaData>();
            mock.Setup(n => n.ReadDetails(It.IsAny<int>()))
                .Returns(() =>
                {
                    var dt = new DataTable();

                    DataColumn dc = new DataColumn("Id", typeof(int));
                    dt.Columns.Add(dc);
                    dc = new DataColumn("UploadDate", typeof(DateTime));
                    dt.Columns.Add(dc);
                    dc = new DataColumn("UploaderName", typeof(string));
                    dt.Columns.Add(dc);
                    dc = new DataColumn("PathToFile", typeof(string));
                    dt.Columns.Add(dc);
                    dc = new DataColumn("Checksum", typeof(string));
                    dt.Columns.Add(dc);

                    DataRow dr = dt.NewRow();
                    dr[0] = "1";
                    dr[1] = "01.02.1999";
                    dr[2] = "abc";
                    dr[3] = "abc";
                    dr[4] = "525852442df4bac81bd87e4377e28710";
                    dt.Rows.Add(dr);

                    dr = dt.NewRow();
                    dr[0] = "2";
                    dr[1] = "01.02.1999";
                    dr[2] = "abc";
                    dr[3] = "abc";
                    dr[4] = "525852442df4bac81bd87e4377e28713";
                    dt.Rows.Add(dr);

                    return dt;
                });
            var fileValidator = new FileValidator(mock.Object);
            // Assert
            Assert.IsTrue(fileValidator.CheckVersionAlreadyExists(1, "525852442df4bac81bd87e4377e28710"));
        }
        [TestMethod]
        public void WhenChecksumNotExists_ReturnFalse()
        {
            Mock<IMetaData> mock = new Mock<IMetaData>();
            mock.Setup(n => n.ReadDetails(It.IsAny<int>()))
                .Returns(() =>
                {
                    var dt = new DataTable();

                    DataColumn dc = new DataColumn("Id", typeof(int));
                    dt.Columns.Add(dc);
                    dc = new DataColumn("UploadDate", typeof(DateTime));
                    dt.Columns.Add(dc);
                    dc = new DataColumn("UploaderName", typeof(string));
                    dt.Columns.Add(dc);
                    dc = new DataColumn("PathToFile", typeof(string));
                    dt.Columns.Add(dc);
                    dc = new DataColumn("Checksum", typeof(string));
                    dt.Columns.Add(dc);

                    DataRow dr = dt.NewRow();
                    dr[0] = "1";
                    dr[1] = "01.02.1999";
                    dr[2] = "abc";
                    dr[3] = "abc";
                    dr[4] = "525852442df4bac81bd87e4377e28713";
                    dt.Rows.Add(dr);

                    dr = dt.NewRow();
                    dr[0] = "1";
                    dr[1] = "01.02.1999";
                    dr[2] = "abc";
                    dr[3] = "abc";
                    dr[4] = "525852442df4bac81bd87e4377e28710";
                    dt.Rows.Add(dr);

                    return dt;
                });
            var fileValidator = new FileValidator(mock.Object);
            // Assert
            Assert.IsFalse(fileValidator.CheckVersionAlreadyExists(1, "525852442df4bac81bd87e4377e28715"));
        }
    }
}
