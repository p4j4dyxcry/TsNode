using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TsNode.Controls;
using TsNode.Controls.Connection;
using TsNode.Controls.Drag;
using TsNode.Controls.Drag.Controller;
using TsNode.Interface;
using Xunit;
using SelectionChangedEventArgs = TsNode.Controls.SelectionChangedEventArgs;

namespace TsNode.Test
{
    public class RectSelection
    {
        [Fact]
        public void SelectTest()
        {
            var nodes = new INodeControl[]
            {
                Mock.CreateNodeControl(0,50,false),
                Mock.CreateNodeControl(0,100,false),
                Mock.CreateNodeControl(0,200,false),
            };

            var controller = BuildController(nodes, null);

            Assert.IsType<RectSelectionController>(controller);

            // 0,0 - 150,150までの矩形選択
            {
                DoDrag( controller , new Point(0,0),new Point(150,150) );
            
                controller.OnDragEnd();
            
                Assert.True(nodes[0].IsSelected);
                Assert.True(nodes[1].IsSelected);
                Assert.False(nodes[2].IsSelected);                
            }

            // 40,120 - 200,200までの矩形選択
            {
                DoDrag( controller , new Point(40,120) ,  new Point(200,200) );
            
                controller.OnDragEnd();
            
                Assert.False(nodes[0].IsSelected);
                Assert.True(nodes[1].IsSelected);
                Assert.True(nodes[2].IsSelected);                
            }
        }
        
        [Fact]
        public void CommandTest()
        {
            var nodes = new INodeControl[]
            {
                Mock.CreateNodeControl(-500,-1500,false),
                Mock.CreateNodeControl(-500,-2000,true),
                Mock.CreateNodeControl(0,100,false),
                Mock.CreateNodeControl(0,200,false),
            };

            var commandExecuted = false;
            var command = Mock.Command<SelectionChangedEventArgs>(x =>
            {
                // コマンドが1回だけ実行されることを確認
                Assert.False(commandExecuted);
                
                // nodes[0]だけは変更がないのでここにくる数は3
                Assert.Equal(3,x.ChangedItems.Length);

                Assert.Contains(nodes[1],x.ChangedItems);
                Assert.Contains(nodes[2],x.ChangedItems);
                Assert.Contains(nodes[3],x.ChangedItems);
                
                commandExecuted = true;
            });
            
            var controller = BuildController(nodes, command);

            Assert.IsType<RectSelectionController>(controller);

            // 0,0 - 150,150までの矩形選択
            {
                DoDrag( controller , new Point(0,150),new Point(30,250) );
            
                controller.OnDragEnd();
            
                Assert.False(nodes[0].IsSelected);
                Assert.False(nodes[1].IsSelected);
                Assert.True(nodes[2].IsSelected);
                Assert.True(nodes[3].IsSelected);                
            }
            
            // コマンドが実行されたことを確認
            Assert.True(commandExecuted);
        }

        /// <summary>
        /// ドラッグコントローラを作成する
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        private IDragController BuildController(INodeControl[] nodes , ICommand selectionChangedCommand)
        {
            var builder = new DragControllerBuilder(null, MouseButton.Left, nodes, Array.Empty<ConnectionShape>());
            var controller = builder
                .AddBuildTarget(new RectSelectionDragBuild(builder,1,null,null))
                .SetSelectionChangedCommand(selectionChangedCommand)
                .Build();

            return controller;
        }

        /// <summary>
        /// ドラッグを開始
        /// </summary>
        /// <param name="controller">ドラッグコントローラ</param>
        /// <param name="start"></param>
        /// <param name="first">1回目のドラッグ位置</param>
        /// <param name="others">2回目以降のドラッグ位置</param>
        private void DoDrag(IDragController controller , Point start , Point first, params Point[] others)
        {
            // start drag
            var startArgs = new DragControllerEventArgs(start,start,new Vector(0,0),MouseButton.Left);
            {
                Assert.True(controller.CanDragStart(startArgs));
            
                controller.OnStartDrag(startArgs);                
            }

            // drag moving
            var firstDragArgs = startArgs.CreateUpdatedArgs(first);
            {
                controller.OnDragMoving(firstDragArgs);
            }
            
            // other dragging
            {
                var args = firstDragArgs;
                foreach (var other in others)
                {
                    args = args.CreateUpdatedArgs(other);
                    controller.OnDragMoving(args);
                }
            }
        }
    }
}