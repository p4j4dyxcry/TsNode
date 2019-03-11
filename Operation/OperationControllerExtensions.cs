using System;
using System.Linq;

namespace TsGui.Operation
{
    public static class OperationControllerExtensions
    {
        public static void MoveTo(this IOperationController controller, IOperation target)
        {
            var isRollBack    = controller.Operations.Contains(target);

            var isRollForward = controller.RollForwardTargets.Contains(target);

            if(isRollBack is false && isRollForward is false)
                return;

            if (isRollBack)
            {
                while (controller.Peek() != target)
                    controller.Undo();
            }

            if (isRollForward)
            {
                while (controller.RollForwardTargets.FirstOrDefault() != target)
                    controller.Redo();
                controller.Redo();
            }
        }

        public static void Distinct(this IOperationController controller, object key)
        {
            _distinct_internal<MergeableOperation>(controller, key,
                (x, y) => MergeableOperation.MakeMerged(x, y, false));
        }

        public static void Distinct<T>(this IOperationController controller, object key)
        {
            _distinct_internal<MergeableOperation<T>>(controller, key,
                (x, y) => MergeableOperation<T>.MakeMerged(x, y, false));
        }

        private static void _distinct_internal<T>(this IOperationController controller, object key,
            Func<T, T, IOperation> generateMergedOperation)
            where T : class, IMergeableOperation
        {
            var operations = controller.Operations.ToList();

            var mergeable = operations
                .OfType<T>()
                .Where(x => x.MergeJudge.GetMergeKey() == key)
                .ToArray();

            var first = mergeable.FirstOrDefault();
            var last = mergeable.LastOrDefault();

            if (first is null || last is null)
                return;

            var lastIndex = 0;
            foreach (var operation in mergeable)
            {
                lastIndex = operations.IndexOf(operation);
                operations.RemoveAt(lastIndex);
            }

            var newOperation = generateMergedOperation(first, last);
            operations.Insert(lastIndex, newOperation);

            controller.Flush();
            operations.ForEach(x => controller.Push(x));
        }

    }
}
