using TsNode.Foundations;
using TsNode.Interface;
using Xunit;

namespace TsNode.Test
{
    public class Selectable
    {
        public class TestSelectable : ISelectable
        {
            public bool IsSelected { get; set; }

            public TestSelectable(bool initializeValue)
            {
                IsSelected = initializeValue;
            }
        }

        [Fact]
        public void ToggleTest()
        {
            var selectables = new[]
            {
                new TestSelectable(true), 
                new TestSelectable(false),                 
            };

            SelectHelper.ToggleSelect(selectables, selectables);
            
            Assert.False(selectables[0].IsSelected);
            Assert.True(selectables[1].IsSelected);
            
            SelectHelper.ToggleSelect(selectables, selectables);
            
            Assert.True(selectables[0].IsSelected);
            Assert.False(selectables[1].IsSelected);
        }
        
        [Fact]
        public void AddSelectTest()
        {
            var selectables = new[]
            {
                new TestSelectable(true), 
                new TestSelectable(false),                 
            };

            SelectHelper.AddSelect(selectables, new []{selectables[1]});
            Assert.All(selectables, result => Assert.True(result.IsSelected));
        }
        
        [Fact]
        public void SingleSelectTest()
        {
            {
                var selectables = new[]
                {
                    new TestSelectable(true), 
                    new TestSelectable(false),                 
                };

                SelectHelper.SingleSelect(selectables, new []{selectables[1]});
            
                Assert.False(selectables[0].IsSelected);
                Assert.True(selectables[1].IsSelected);
            
                SelectHelper.SingleSelect(selectables, new []{selectables[0]});
            
                Assert.True(selectables[0].IsSelected);
                Assert.False(selectables[1].IsSelected);                
            }

            {
                var selectables = new[]
                {
                    new TestSelectable(true), 
                    new TestSelectable(true),                 
                };
            
                SelectHelper.SingleSelect(selectables, new []{selectables[0]});
            
                Assert.True(selectables[0].IsSelected);
                Assert.True(selectables[1].IsSelected);                
            }
        }
        
        [Fact]
        public void OnlySelectTest()
        {
            {
                var selectables = new[]
                {
                    new TestSelectable(true), 
                    new TestSelectable(false),                 
                };

                SelectHelper.OnlySelect(selectables, new []{selectables[1]});
            
                Assert.False(selectables[0].IsSelected);
                Assert.True(selectables[1].IsSelected);
            
                SelectHelper.OnlySelect(selectables, new []{selectables[0]});
            
                Assert.True(selectables[0].IsSelected);
                Assert.False(selectables[1].IsSelected);                
            }

            {
                var selectables = new[]
                {
                    new TestSelectable(true), 
                    new TestSelectable(true),                 
                };

                // SingleSelectとの違いはここ
                SelectHelper.OnlySelect(selectables, new []{selectables[0]});
            
                Assert.True(selectables[0].IsSelected);
                Assert.False(selectables[1].IsSelected);                
            }
        }
    }
}