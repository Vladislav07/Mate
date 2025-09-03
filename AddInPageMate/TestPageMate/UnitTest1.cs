using AddInPageMate;

namespace TestPageMate
{
    public class Tests
    {
        List<ElementSW> list;
        [SetUp]
        public void Setup()
        {
            list = LocalStorage.ReadComponents();
        }

        [Test]
        public void Test1()
        {
            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(list[0].nameSwComponent, "CUBY-00347813");
        }
    }
}