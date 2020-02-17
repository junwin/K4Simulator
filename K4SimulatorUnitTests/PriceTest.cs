using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KaiTrade.Interfaces;
using K4ServiceInterface;
using K4Channel;
using log4net;
using DriverBase;
//using K2DomainSvc;
using Moq;

namespace SimulatorTest
{
    [TestClass]
    public class PriceTest
    {
        KTASimulator.KTASimulator _driver = null;
        private L1PriceSupport.MemoryPriceHandler _priceHandler = null;
        private ILog log = new Mock<ILog>().Object;
        ProductManager pm = null;
        IDriverManager dm = null;
        IVenueSvc vs = null;
        IPublisher pub = null;
        IOrderSvc ordersvc = null;
       

        public PriceTest()
        {
            pm = new ProductManager(log);
            dm = null;
            //vs = new VenueSvc(dm);
            //pub = new Publisher(log);
            //ordersvc = new OrderSvc(pub,vs,log);

            _driver = new KTASimulator.KTASimulator(log);
            _driver.ProductManager = pm;
        }
        [TestMethod]
        public void SetupDriver()
        {
            _driver.Start("");
            //_driver.st
        }

        [TestMethod]
        [DeploymentItem(@"TestData\AAPL_data.csv", "TestData")]
        public void TestPriceFile()
        {
            KTASimulator.FilePriceSource priceSrc = new KTASimulator.FilePriceSource(_driver, log);

            priceSrc.SetUpProductNoRun(@"TestData\AAPL_data.csv");

            List<KaiTrade.Interfaces.IProduct> products = _driver.ProductManager.GetProducts("KTSIM", "", "");
            Assert.AreEqual(products.Count, 1);
        }


        /// <summary>
        /// Test that the price source function works 
        /// </summary>
        [TestMethod]
        [DeploymentItem(@"TestData\AAPL_data.csv", "TestData")]
        public void TestPriceFileStart()
        {
            KTASimulator.FilePriceSource priceSrc = new KTASimulator.FilePriceSource(_driver, log);

            priceSrc.PriceUpdate += new PriceUpdate(this.PriceUpdate);

            priceSrc.Start(@"TestData\AAPL_data.csv");

            ////List<KaiTrade.Interfaces.IProduct> products = _driver.Facade.ProductManager.GetProducts("KTSIM", "", "");
           // Assert.AreEqual(products.Count, 1);
        }

        /// <summary>
        /// Tests that price files are loaded and run if available when the
        /// sim driver stats - should start DELL & AAPL
        /// </summary>
        [TestMethod]
        [DeploymentItem(@"TestData\AAPL_data.csv", "TestData")]
        [DeploymentItem(@"TestData\DELL_data.csv", "TestData")]
        public void TestPriceFileStartDriverInit()
        {
            // HPQ will just fill over time an order of 5 will fill 2,1,2
            //_driver = new KTASimulator.KTASimulator(new DriverBase.AppFacade(null, null), log);
            //__driver = new KTASimulator.KTASimulator(new DriverBase.AppFacade(pm, ordersvc), log);
            string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            path = System.IO.Path.GetDirectoryName(path);
            _driver.AppPath = path;

            _driver.ProductUpdate += new ProductUpdate(ProductUpdate);
            
            _priceHandler = new L1PriceSupport.MemoryPriceHandler(log);
            //_driver.Facade.PriceHandler = _priceHandler;

            // Gets driver messages (fills, Accounts etc)
            _driver.Message += new NewMessage(OnMessage);

            (_driver as DriverBase.DriverBase).PriceUpdate += new K4ServiceInterface.PriceUpdate(this.PriceUpdate);

            _driver.Start("");

            // Test that S.DELL is set up as expected
            IProduct product = pm.GetProductMnemonic("S.DELL");
            _priceHandler.GetPXPublisher(product);

            _driver.OpenPrices(product, 0, DateTime.Now.Ticks.ToString());
            
           
         
            System.Threading.Thread.Sleep(5000);
             
            List<KaiTrade.Interfaces.IProduct> products = pm.GetProducts("KTSIM", "", "");
            Assert.AreEqual(2, products.Count);

            var pub = _priceHandler.GetPXPublisher(product);

        }

        void OnMessage(IMessage message)
        {
            
        }

        public void PriceUpdate(KaiTrade.Interfaces.IPXUpdate pxUpdate)
        {
            if (_priceHandler != null)
            {
                _priceHandler.ApplyPriceUpdate(pxUpdate);
            }
        }

        public void ProductUpdate(object sender, IProduct product)
        {
            pm.AddProduct(product);
        }
        
    }
}
