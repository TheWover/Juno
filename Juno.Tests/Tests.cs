using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit;

namespace Juno.Tests
{
    public class TestClass1
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public int TestMethod(int a, int b)
        {
            return a + b;
        }
    } 
    
    public class TestClass2
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public int TestMethod(int a, int b)
        {
            return a * b;
        }
    }
    
    public class Tests
    {
        private readonly FunctionDetour _functionDetour;
        
        private readonly TestClass1 _testClass;
        
        private readonly int _testVariable1;
        
        private readonly int _testVariable2;
        
        public Tests()
        {
            // Initialize a function detour instance
            
            _functionDetour = new FunctionDetour(typeof(TestClass1), "TestMethod", typeof(TestClass2), "TestMethod");
            
            // Initialize a test class
            
            _testClass = new TestClass1();
            
            // Initialise test variables
            
            _testVariable1 = 5;
            
            _testVariable2 = 10;
        }

        [Fact]
        public void TestDetourUsingMethodInfo()
        {
            var originalMethodInfo = typeof(TestClass1).GetMethod("TestMethod", BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

            var targetMethodInfo = typeof(TestClass2).GetMethod("TestMethod", BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            // Initialize a function detour instance

            var testDetour = new FunctionDetour(originalMethodInfo, targetMethodInfo);

            // Initialize a test class

            var testClass1 = new TestClass1();

            Assert.Equal(3, testClass1.TestMethod(1, 2));

            testDetour.AddDetour();

            Assert.Equal(2, testClass1.TestMethod(1, 2));

            testDetour.RemoveDetour();
        }

        [Fact]
        public void TestAddDetour()
        {
            // Add the detour
            
            _functionDetour.AddDetour();
            
            Assert.Equal(50, _testClass.TestMethod(_testVariable1, _testVariable2));
           
            // Remove the detour
            
            _functionDetour.RemoveDetour();
        }
        
        [Fact]
        public void TestRemoteDetour()
        {
            // Add the detour
            
            _functionDetour.AddDetour();
            
            // Remove the detour
            
            _functionDetour.RemoveDetour();
            
            Assert.Equal(15, _testClass.TestMethod(_testVariable1, _testVariable2));
        }
    }
}