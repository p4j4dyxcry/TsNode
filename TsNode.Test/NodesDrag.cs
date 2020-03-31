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

namespace TsNode.Test
{
    public class NodesDrag
    {
        [Fact]
        public void DragSingleNodeTest()
        {
            var nodes = new INodeControl[]
            {
                Mock.CreateNodeControl(0,50,true),
            };

            var executedCompleteCommand = false;
            var nodeDragCompletedCommand = Mock.Command<CompletedMoveNodeEventArgs>(x =>
            {
                AssertCheckNodeMovePoint(x, nodes[0], 0, 50, 10, 60);

                executedCompleteCommand = true;
            });

            var controller = BuildController(nodes, nodeDragCompletedCommand);

            Assert.IsType<NodesDragController>(controller);
            
            var args = DoDrag( controller , new Point(10,10) );
            
            controller.OnDragEnd(args.CreateEndArgs());
            
            Assert.True(executedCompleteCommand);
        }

        [Fact]
        public void DragMultiNodeTest()
        {
            var nodes = new INodeControl[]
            {
                Mock.CreateNodeControl(-20,0,true),
                Mock.CreateNodeControl(0,0,false),
                Mock.CreateNodeControl(10,-50,true),
            };

            var executedCompleteCommand = false;
            var nodeDragCompletedCommand = Mock.Command<CompletedMoveNodeEventArgs>(x =>
            {
                // selected が falseなものはドラッグされていない
                Assert.Equal(2,x.InitialNodePoints.Count);

                Assert.Equal(0,nodes[1].X);
                Assert.Equal(0,nodes[1].Y);
                
                AssertCheckNodeMovePoint(x, nodes[0], -20, 0, -10, 10);
                
                AssertCheckNodeMovePoint(x, nodes[2], 10, -50, 20, -40);

                executedCompleteCommand = true;
            });

            var controller = BuildController(nodes, nodeDragCompletedCommand);

            var args = DoDrag( controller , new Point(10,10));
            controller.OnDragEnd(args.CreateEndArgs());
            
            Assert.True(executedCompleteCommand);
        }

        [Fact]
        public void ChainDragTest()
        {
            var nodes = new INodeControl[]
            {
                Mock.CreateNodeControl(0,0,true),
            };

            var executedCompleteCommand = false;
            var nodeDragCompletedCommand = Mock.Command<CompletedMoveNodeEventArgs>(x =>
            {
                AssertCheckNodeMovePoint(x, nodes[0], 0, 0, 30, 0);

                executedCompleteCommand = true;
            });

            var controller = BuildController(nodes, nodeDragCompletedCommand);

            // 10,10 → -5,-5 → 30,0 へ順番に動かす
            var args = DoDrag( controller , new Point(10,10) , new Point(-5,-5) , new Point(30,0));
            controller.OnDragEnd(args.CreateEndArgs());
            
            Assert.True(executedCompleteCommand);
        }
        
        [Fact]
        public void FailedTest()
        {
            var nodes = new INodeControl[]
            {
                Mock.CreateNodeControl(0,0,false),
            };

            var controller = BuildController(nodes, null);
            
            Assert.Null(controller);
        }

        /// <summary>
        /// ドラッグコントローラを作成する
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        private IDragController BuildController(INodeControl[] nodes , ICommand command)
        {
            var builder = new DragControllerBuilder(null, MouseButton.Left, nodes, Array.Empty<ConnectionShape>());
            var controller = builder
                .AddBuildTarget(new NodesDragBuild(builder, 1, false,0))
                .SetNodeDragCompletedCommand(command)
                .Build();

            return controller;
        }

        /// <summary>
        /// ノード座標をチェック
        /// </summary>
        /// <param name="args"></param>
        /// <param name="node"></param>
        /// <param name="originX">元のx</param>
        /// <param name="originY">元のy</param>
        /// <param name="movedX">移動後のx</param>
        /// <param name="movedY">移動後のy</param>
        private void AssertCheckNodeMovePoint(CompletedMoveNodeEventArgs args , INodeControl node, double originX, double originY , double movedX , double movedY)
        {
            var prevPoint = args.InitialNodePoints[node.DataContext as INodeDataContext];
            var endPoint  = args.CompletedNodePoints[node.DataContext as INodeDataContext];
                
            Assert.Equal(originX ,prevPoint.X);
            Assert.Equal(originY,prevPoint.Y);
                
            Assert.Equal(movedX ,endPoint.X);
            Assert.Equal(movedY,endPoint.Y);
                
            Assert.Equal(movedX ,node.X);
            Assert.Equal(movedY, node.Y);
        }

        /// <summary>
        /// ドラッグを開始
        /// </summary>
        /// <param name="controller">ドラッグコントローラ</param>
        /// <param name="first">1回目のドラッグ位置</param>
        /// <param name="others">2回目以降のドラッグ位置</param>
        private DragControllerEventArgs DoDrag(IDragController controller , Point first, params Point[] others)
        {
            // start drag
            var startArgs = new DragControllerEventArgs(new Point(0,0),new Point(0,0),new Vector(0,0),MouseButton.Left);
            {
                Assert.True(controller.CanDragStart(startArgs));
            
                controller.OnStartDrag(startArgs);                
            }

            // drag moving
            var firstDragArgs = startArgs.CreateUpdatedArgs(first);
            {
                controller.OnDragMoving(firstDragArgs);
            }
            
            var args = firstDragArgs;
            // other dragging
            {
                foreach (var other in others)
                {
                    args = args.CreateUpdatedArgs(other);
                    controller.OnDragMoving(args);
                }
            }

            return args;
        }
    }
}