using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebSockets.UI;
using WebSockets.UI.Controllers;

namespace WebSockets.UI.Tests.Controllers
{
    [TestClass]
    public class HomeControllerTest
    {
        [TestMethod]
        public void Draw()
        {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.Draw() as ViewResult;

            // Assert
            Assert.AreEqual("Draw", result.ViewBag.Title);
        }

        [TestMethod]
        public void Chat()
        {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.Draw() as ViewResult;

            // Assert
            Assert.AreEqual("Chat", result.ViewBag.Title);
        }
    }
}
