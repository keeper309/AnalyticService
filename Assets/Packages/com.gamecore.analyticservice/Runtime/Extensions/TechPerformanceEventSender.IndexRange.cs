using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using GameCore.GeneralExtensions;
using UnityEngine;
using ILogger = GameCore.LoggerService.ILogger;

namespace GameCore.AnalyticService
{
    public partial class TechPerformanceEventSender
    {
        /// <summary>
        ///     Range for lists [Min..Max]
        /// </summary>
        private readonly struct IndexRange
        {
            public readonly int Min; // Included
            public readonly int Max; // Included

            public int MiddleId => (Min + Max) / 2;
            public int IdsCount => Max - Min + 1;

            public IndexRange(int min, int max)
            {
                Min = min;
                Max = max;
            }

            public int Clamp(int value)
            {
                return Mathf.Clamp(value, Min, Max);
            }

            public bool IsInRange(int value)
            {
                return value >= Min && value <= Max;
            }

            public static IndexRange FromList<TReadOnlyList>(TReadOnlyList list) where TReadOnlyList : IReadOnlyList<float>
            {
                if (list.IsNullOrEmpty())
                {
                    throw new InvalidRangeOperationException($"list is empty for {nameof(FromList)}");
                }

                return new IndexRange(0, list.Count - 1);
            }

            public static IndexRange FromListAndFactors<TReadOnlyList>(
                ILogger logger,
                TReadOnlyList list,
                float minFactor,
                float maxFactor
            )
                where TReadOnlyList : IReadOnlyList<float>
            {
                if (list.IsNullOrEmpty())
                {
                    throw new InvalidRangeOperationException($"list is empty for {nameof(FromListAndFactors)}");
                }

                ValidateRangeFactors(minFactor, maxFactor);

                int maxListId = list.Count - 1;
                IndexRange range = new IndexRange(
                    Mathf.FloorToInt(minFactor * maxListId),
                    Mathf.CeilToInt(maxFactor * maxListId)
                );

                range.Validate(logger, list);

                return range;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Validate<TReadOnlyList>(ILogger logger, TReadOnlyList list) where TReadOnlyList : IReadOnlyList<float>
            {
                logger.Assert(
                    Min <= Max &&
                    Min >= 0 &&
                    list.Count > Max &&
                    list.Count > 0,
                    $"Invalid Range [{Min}..{Max}] or ListCount: {list.Count}"
                );
            }

            public static void ValidateRangeFactors(float minFactor, float maxFactor)
            {
                if (minFactor < 0 ||
                    maxFactor > 1 ||
                    maxFactor < minFactor)
                {
                    throw new InvalidRangeOperationException($"Invalid factor range [{minFactor} .. {maxFactor}]");
                }
            }
        }

        public class InvalidRangeOperationException : Exception
        {
            public InvalidRangeOperationException(string message)
                : base(message) { }
        }
    }
}
