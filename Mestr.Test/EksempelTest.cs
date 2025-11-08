namespace Mestr.Test
{
    public class UnitTest1
    {
        public static int Add(int a, int b) => a + b;
        
        [Fact]
        public void Good() =>         
            Assert.Equal(4, Add(2, 2));

        [Fact]
        public void Bad() =>     
            Assert.Equal(5, Add(2, 2));
    }

}
