using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mane.Unity
{
    /// <summary>
    /// Shared easing presets as <see cref="AnimationCurve"/> values on the range 0–1.
    /// Time is the key, value is the eased weight. Curves follow Robert Penner's equations.
    /// Back, Elastic, and Spring overshoot that range; every curve still starts at 0 and ends at 1.
    /// The instances are shared: do not edit their keys. Use <see cref="Clone"/> for a private copy.
    /// </summary>
    public static class Easing
    {
        private const float BackOvershoot = 1.70158f;
        private const float BackInOutOvershoot = 1.70158f * 1.525f;
        private const float BounceScale = 7.5625f;
        private const float BounceWidth = 2.75f;
        private const float ElasticPeriod = .3f;
        private const float FitTolerance = .001f;

        /// <summary>Constant speed from 0 to 1.</summary>
        public static AnimationCurve Linear { get; } = Curve(
            new Keyframe(0f, 0f, 1f, 1f),
            new Keyframe(1f, 1f, 1f, 1f));

        /// <summary>Quadratic ease-in. Slow start.</summary>
        public static AnimationCurve QuadIn { get; } = Curve(
            new Keyframe(0f, 0f, 0f, 0f),
            new Keyframe(1f, 1f, 2f, 2f));

        /// <summary>Quadratic ease-out. Slow end.</summary>
        public static AnimationCurve QuadOut { get; } = Curve(
            new Keyframe(0f, 0f, 2f, 2f),
            new Keyframe(1f, 1f, 0f, 0f));

        /// <summary>Quadratic ease-in-out.</summary>
        public static AnimationCurve QuadInOut { get; } = Curve(
            new Keyframe(0f, 0f, 0f, 0f),
            new Keyframe(.5f, .5f, 2f, 2f),
            new Keyframe(1f, 1f, 0f, 0f));

        /// <summary>Cubic ease-in. Slow start.</summary>
        public static AnimationCurve CubicIn { get; } = Curve(
            new Keyframe(0f, 0f, 0f, 0f),
            new Keyframe(1f, 1f, 3f, 3f));

        /// <summary>Cubic ease-out. Slow end.</summary>
        public static AnimationCurve CubicOut { get; } = Curve(
            new Keyframe(0f, 0f, 3f, 3f),
            new Keyframe(1f, 1f, 0f, 0f));

        /// <summary>Cubic ease-in-out.</summary>
        public static AnimationCurve CubicInOut { get; } = Curve(
            new Keyframe(0f, 0f, 0f, 0f),
            new Keyframe(.5f, .5f, 3f, 3f),
            new Keyframe(1f, 1f, 0f, 0f));

        /// <summary>Quartic ease-in. Slow start.</summary>
        public static AnimationCurve QuartIn { get; } = Fit(QuartInAt);

        /// <summary>Quartic ease-out. Slow end.</summary>
        public static AnimationCurve QuartOut { get; } = Fit(QuartOutAt);

        /// <summary>Quartic ease-in-out.</summary>
        public static AnimationCurve QuartInOut { get; } = Fit(QuartInOutAt, .5f);

        /// <summary>Quintic ease-in. Slow start.</summary>
        public static AnimationCurve QuintIn { get; } = Fit(QuintInAt);

        /// <summary>Quintic ease-out. Slow end.</summary>
        public static AnimationCurve QuintOut { get; } = Fit(QuintOutAt);

        /// <summary>Quintic ease-in-out.</summary>
        public static AnimationCurve QuintInOut { get; } = Fit(QuintInOutAt, .5f);

        /// <summary>Sine ease-in. Slow start.</summary>
        public static AnimationCurve SinIn { get; } = Fit(SinInAt);

        /// <summary>Sine ease-out. Slow end.</summary>
        public static AnimationCurve SinOut { get; } = Fit(SinOutAt);

        /// <summary>Sine ease-in-out.</summary>
        public static AnimationCurve SinInOut { get; } = Fit(SinInOutAt, .5f);

        /// <summary>Exponential ease-in. Slow start.</summary>
        public static AnimationCurve ExpoIn { get; } = Fit(ExpoInAt);

        /// <summary>Exponential ease-out. Slow end.</summary>
        public static AnimationCurve ExpoOut { get; } = Fit(ExpoOutAt);

        /// <summary>Exponential ease-in-out.</summary>
        public static AnimationCurve ExpoInOut { get; } = Fit(ExpoInOutAt, .5f);

        /// <summary>Circular ease-in. Slow start, steep end.</summary>
        public static AnimationCurve CircIn { get; } = Fit(CircInAt);

        /// <summary>Circular ease-out. Steep start, slow end.</summary>
        public static AnimationCurve CircOut { get; } = Fit(CircOutAt);

        /// <summary>Circular ease-in-out. Steep middle.</summary>
        public static AnimationCurve CircInOut { get; } = Fit(CircInOutAt, .5f);

        /// <summary>Back ease-in. Pulls back past 0, then goes to 1.</summary>
        public static AnimationCurve BackIn { get; } = Curve(
            new Keyframe(0f, 0f, 0f, 0f),
            new Keyframe(1f, 1f, BackOvershoot + 3f, BackOvershoot + 3f));

        /// <summary>Back ease-out. Overshoots 1, then settles.</summary>
        public static AnimationCurve BackOut { get; } = Curve(
            new Keyframe(0f, 0f, BackOvershoot + 3f, BackOvershoot + 3f),
            new Keyframe(1f, 1f, 0f, 0f));

        /// <summary>Back ease-in-out. Overshoots both ends.</summary>
        public static AnimationCurve BackInOut { get; } = Curve(
            new Keyframe(0f, 0f, 0f, 0f),
            new Keyframe(.5f, .5f, BackInOutOvershoot + 3f, BackInOutOvershoot + 3f),
            new Keyframe(1f, 1f, 0f, 0f));

        /// <summary>Bounce ease-in. Small bounces, then a launch to 1.</summary>
        public static AnimationCurve BounceIn { get; } = Mirror(CreateBounceOut());

        /// <summary>Bounce ease-out. Lands, then a few decaying bounces.</summary>
        public static AnimationCurve BounceOut { get; } = CreateBounceOut();

        /// <summary>Bounce ease-in-out.</summary>
        public static AnimationCurve BounceInOut { get; } = CreateBounceInOut();

        /// <summary>Elastic ease-in. Oscillates around 0, then snaps to 1.</summary>
        public static AnimationCurve ElasticIn { get; } = Fit(ElasticInAt);

        /// <summary>Elastic ease-out. Overshoots 1, then oscillates down.</summary>
        public static AnimationCurve ElasticOut { get; } = Fit(ElasticOutAt);

        /// <summary>Elastic ease-in-out.</summary>
        public static AnimationCurve ElasticInOut { get; } = Fit(ElasticInOutAt, .5f);

        /// <summary>Spring settle from 0 to 1, with a small overshoot.</summary>
        public static AnimationCurve Spring { get; } = Fit(SpringAt);

        /// <summary>
        /// Returns a new curve with the same keys and wrap mode as <paramref name="curve"/>.
        /// </summary>
        public static AnimationCurve Clone(AnimationCurve curve)
        {
            if (curve == null)
                throw new ArgumentNullException(nameof(curve));

            return new AnimationCurve(curve.keys)
            {
                preWrapMode = curve.preWrapMode,
                postWrapMode = curve.postWrapMode
            };
        }


        private static float QuartInAt(float t) => t * t * t * t;

        private static float QuartOutAt(float t)
        {
            float v = t - 1f;
            return -(v * v * v * v - 1f);
        }

        private static float QuartInOutAt(float t)
        {
            float v = t * 2f;
            if (v < 1f)
                return .5f * v * v * v * v;

            v -= 2f;
            return -.5f * (v * v * v * v - 2f);
        }

        private static float QuintInAt(float t) => t * t * t * t * t;

        private static float QuintOutAt(float t)
        {
            float v = t - 1f;
            return v * v * v * v * v + 1f;
        }

        private static float QuintInOutAt(float t)
        {
            float v = t * 2f;
            if (v < 1f)
                return .5f * v * v * v * v * v;

            v -= 2f;
            return .5f * (v * v * v * v * v + 2f);
        }

        private static float SinInAt(float t) => 1f - Mathf.Cos(t * Mathf.PI * .5f);

        private static float SinOutAt(float t) => Mathf.Sin(t * Mathf.PI * .5f);

        private static float SinInOutAt(float t) => .5f * (1f - Mathf.Cos(Mathf.PI * t));

        private static float ExpoInAt(float t)
        {
            if (t <= 0f)
                return 0f;
            if (t >= 1f)
                return 1f;

            return Mathf.Pow(2f, 10f * (t - 1f));
        }

        private static float ExpoOutAt(float t)
        {
            if (t <= 0f)
                return 0f;
            if (t >= 1f)
                return 1f;

            return 1f - Mathf.Pow(2f, -10f * t);
        }

        private static float ExpoInOutAt(float t)
        {
            if (t <= 0f)
                return 0f;
            if (t >= 1f)
                return 1f;

            float v = t * 2f;
            if (v < 1f)
                return .5f * Mathf.Pow(2f, 10f * (v - 1f));

            v -= 1f;
            return .5f * (2f - Mathf.Pow(2f, -10f * v));
        }

        private static float CircInAt(float t)
        {
            t = Mathf.Clamp01(t);
            return 1f - Mathf.Sqrt(1f - t * t);
        }

        private static float CircOutAt(float t)
        {
            float v = t - 1f;
            return Mathf.Sqrt(Mathf.Max(0f, 1f - v * v));
        }

        private static float CircInOutAt(float t)
        {
            float v = t * 2f;
            if (v < 1f)
                return -.5f * (Mathf.Sqrt(Mathf.Max(0f, 1f - v * v)) - 1f);

            v -= 2f;
            return .5f * (Mathf.Sqrt(Mathf.Max(0f, 1f - v * v)) + 1f);
        }

        private static float ElasticInAt(float t) => 1f - ElasticOutAt(1f - t);

        // Phase is period/4 so the halves meet. period/5 leaves a gap at the join.
        private static float ElasticOutAt(float t)
        {
            if (t <= 0f)
                return 0f;
            if (t >= 1f)
                return 1f;

            float phase = ElasticPeriod * .25f;
            return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t - phase) * (Mathf.PI * 2f) / ElasticPeriod) + 1f;
        }

        private static float ElasticInOutAt(float t)
        {
            if (t <= 0f)
                return 0f;
            if (t >= 1f)
                return 1f;

            float phase = ElasticPeriod * .25f;
            float v = t * 2f;
            if (v < 1f)
            {
                v -= 1f;
                return -.5f * Mathf.Pow(2f, 10f * v) * Mathf.Sin((v - phase) * (Mathf.PI * 2f) / ElasticPeriod);
            }

            v -= 1f;
            return Mathf.Pow(2f, -10f * v) * Mathf.Sin((v - phase) * (Mathf.PI * 2f) / ElasticPeriod) * .5f + 1f;
        }

        private static float SpringAt(float t)
        {
            t = Mathf.Clamp01(t);
            float wave = Mathf.Sin(t * Mathf.PI * (.2f + 2.5f * t * t * t));
            return (wave * Mathf.Pow(1f - t, 2.2f) + t) * (1f + 1.2f * (1f - t));
        }

        private static float BounceOutAt(float t)
        {
            if (t < 1f / BounceWidth)
                return BounceScale * t * t;

            if (t < 2f / BounceWidth)
            {
                t -= 1.5f / BounceWidth;
                return BounceScale * t * t + .75f;
            }

            if (t < 2.5f / BounceWidth)
            {
                t -= 2.25f / BounceWidth;
                return BounceScale * t * t + .9375f;
            }

            t -= 2.625f / BounceWidth;
            return BounceScale * t * t + .984375f;
        }

        private static AnimationCurve CreateBounceOut()
        {
            float[] knots =
            {
                0f,
                1f / BounceWidth,
                2f / BounceWidth,
                2.5f / BounceWidth,
                1f
            };

            Keyframe[] keys = new Keyframe[knots.Length];
            for (int i = 0; i < knots.Length; i++)
            {
                float time = knots[i];
                keys[i] = new Keyframe(
                    time,
                    BounceOutAt(time),
                    BounceOutSlope(time, false),
                    BounceOutSlope(time, true));
            }

            return Curve(keys);
        }

        private static float BounceOutSlope(float t, bool right)
        {
            float first = 1f / BounceWidth;
            float second = 2f / BounceWidth;
            float third = 2.5f / BounceWidth;

            if (right)
            {
                if (t < first)
                    return 2f * BounceScale * t;
                if (t < second)
                    return 2f * BounceScale * (t - 1.5f / BounceWidth);
                if (t < third)
                    return 2f * BounceScale * (t - 2.25f / BounceWidth);

                return 2f * BounceScale * (t - 2.625f / BounceWidth);
            }

            if (t <= first)
                return 2f * BounceScale * t;
            if (t <= second)
                return 2f * BounceScale * (t - 1.5f / BounceWidth);
            if (t <= third)
                return 2f * BounceScale * (t - 2.25f / BounceWidth);

            return 2f * BounceScale * (t - 2.625f / BounceWidth);
        }

        private static AnimationCurve CreateBounceInOut()
        {
            AnimationCurve bounceOut = CreateBounceOut();
            AnimationCurve bounceIn = Mirror(bounceOut);
            Keyframe[] incoming = bounceIn.keys;
            Keyframe[] outgoing = bounceOut.keys;
            Keyframe[] keys = new Keyframe[incoming.Length + outgoing.Length - 1];

            for (int i = 0; i < incoming.Length; i++)
            {
                Keyframe key = incoming[i];
                keys[i] = new Keyframe(key.time * .5f, key.value * .5f, key.inTangent, key.outTangent);
            }

            for (int i = 1; i < outgoing.Length; i++)
            {
                Keyframe key = outgoing[i];
                keys[incoming.Length + i - 1] = new Keyframe(
                    (key.time + 1f) * .5f,
                    key.value * .5f + .5f,
                    key.inTangent,
                    key.outTangent);
            }

            return Curve(keys);
        }

        private static AnimationCurve Mirror(AnimationCurve source)
        {
            Keyframe[] src = source.keys;
            Keyframe[] keys = new Keyframe[src.Length];
            for (int i = 0; i < src.Length; i++)
            {
                Keyframe key = src[src.Length - 1 - i];
                keys[i] = new Keyframe(1f - key.time, 1f - key.value, key.outTangent, key.inTangent);
            }

            return Curve(keys);
        }

        // Cubic Hermite fit. Tangents are slopes, clamped to three times the segment secant
        // so a steep end (circular, exponential) cannot ring. Keys are added until the miss is about .001.
        private static AnimationCurve Fit(Func<float, float> ease, params float[] seeds)
        {
            const int maxKeys = 48;
            const int samples = 12;

            List<float> times = new List<float> { 0f, 1f };
            if (seeds != null)
            {
                for (int i = 0; i < seeds.Length; i++)
                    times.Add(seeds[i]);
            }

            SeedExtrema(ease, times);
            times = Dedupe(times);

            for (int pass = 0; pass < 16 && times.Count < maxKeys; pass++)
            {
                Keyframe[] keys = BuildKeys(ease, times);
                List<float> inserts = new List<float>();
                float worst = WorstError(ease, keys, samples, inserts);
                if (worst <= FitTolerance)
                    break;

                int before = times.Count;
                int room = maxKeys - before;
                for (int i = 0; i < inserts.Count && i < room; i++)
                    times.Add(inserts[i]);

                times = Dedupe(times);
                if (times.Count == before)
                    break;
            }

            return Curve(BuildKeys(ease, times));
        }

        private static void SeedExtrema(Func<float, float> ease, List<float> times)
        {
            const int steps = 96;
            const float minDelta = .00001f;

            float previous = ease(0f);
            float current = ease(1f / steps);
            for (int i = 1; i < steps; i++)
            {
                float next = ease((i + 1f) / steps);
                float before = current - previous;
                float after = next - current;
                if (before * after < 0f && Mathf.Abs(before) > minDelta && Mathf.Abs(after) > minDelta)
                {
                    float low = (i - 1f) / steps;
                    float high = (i + 1f) / steps;
                    float sign = current >= previous ? 1f : -1f;
                    for (int iteration = 0; iteration < 12; iteration++)
                    {
                        float left = (2f * low + high) / 3f;
                        float right = (low + 2f * high) / 3f;
                        if (sign * ease(left) > sign * ease(right))
                            high = right;
                        else
                            low = left;
                    }

                    times.Add((low + high) * .5f);
                }

                previous = current;
                current = next;
            }
        }

        private static List<float> Dedupe(List<float> times)
        {
            times.Sort();
            List<float> unique = new List<float> { 0f };
            for (int i = 0; i < times.Count; i++)
            {
                float time = Mathf.Clamp01(times[i]);
                if (time - unique[unique.Count - 1] > .00001f)
                    unique.Add(time);
            }

            if (unique[unique.Count - 1] < 1f)
                unique.Add(1f);
            else
                unique[unique.Count - 1] = 1f;

            return unique;
        }

        private static Keyframe[] BuildKeys(Func<float, float> ease, List<float> times)
        {
            int count = times.Count;
            float[] values = new float[count];
            float[] inTangents = new float[count];
            float[] outTangents = new float[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = ease(times[i]);
                Slopes(ease, times[i], out float incoming, out float outgoing);
                inTangents[i] = incoming;
                outTangents[i] = outgoing;
            }

            const float clamp = 3f;
            for (int i = 0; i < count - 1; i++)
            {
                float dx = times[i + 1] - times[i];
                float delta = (values[i + 1] - values[i]) / dx;
                ClampTangent(outTangents, i, delta, clamp);
                ClampTangent(inTangents, i + 1, delta, clamp);
            }

            inTangents[0] = outTangents[0];
            outTangents[count - 1] = inTangents[count - 1];

            Keyframe[] keys = new Keyframe[count];
            for (int i = 0; i < count; i++)
                keys[i] = new Keyframe(times[i], values[i], inTangents[i], outTangents[i]);

            return keys;
        }

        private static void ClampTangent(float[] tangents, int index, float delta, float clamp)
        {
            float tangent = tangents[index];
            if (Mathf.Abs(delta) < .00000001f || tangent * delta <= 0f)
            {
                tangents[index] = 0f;
                return;
            }

            float limit = clamp * Mathf.Abs(delta);
            if (Mathf.Abs(tangent) > limit)
                tangents[index] = Mathf.Sign(tangent) * limit;
        }

        private static void Slopes(Func<float, float> ease, float t, out float inTangent, out float outTangent)
        {
            const float h = .0001f;
            if (t <= h)
            {
                float slope = (ease(Mathf.Min(1f, t + h)) - ease(t)) / h;
                inTangent = slope;
                outTangent = slope;
                return;
            }

            if (t >= 1f - h)
            {
                float slope = (ease(t) - ease(Mathf.Max(0f, t - h))) / h;
                inTangent = slope;
                outTangent = slope;
                return;
            }

            inTangent = (ease(t) - ease(t - h)) / h;
            float right = Mathf.Min(1f, t + h);
            outTangent = (ease(right) - ease(t)) / (right - t);
        }

        private static float WorstError(Func<float, float> ease, Keyframe[] keys, int samples, List<float> inserts)
        {
            float worst = 0f;
            for (int i = 0; i < keys.Length - 1; i++)
            {
                Keyframe left = keys[i];
                Keyframe right = keys[i + 1];
                float segmentWorst = 0f;
                float segmentTime = left.time;
                float span = right.time - left.time;
                for (int s = 1; s < samples; s++)
                {
                    float time = left.time + span * (s / (float)samples);
                    float error = Mathf.Abs(Hermite(time, left, right) - ease(time));
                    if (error > segmentWorst)
                    {
                        segmentWorst = error;
                        segmentTime = time;
                    }
                }

                if (segmentWorst > worst)
                    worst = segmentWorst;
                if (segmentWorst > FitTolerance)
                    inserts.Add(segmentTime);
            }

            return worst;
        }

        private static float Hermite(float time, Keyframe left, Keyframe right)
        {
            float dx = right.time - left.time;
            float u = (time - left.time) / dx;
            float u2 = u * u;
            float u3 = u2 * u;
            return (2f * u3 - 3f * u2 + 1f) * left.value
                + (u3 - 2f * u2 + u) * left.outTangent * dx
                + (-2f * u3 + 3f * u2) * right.value
                + (u3 - u2) * right.inTangent * dx;
        }

        private static AnimationCurve Curve(params Keyframe[] keys) =>
            new(keys)
            {
                preWrapMode = WrapMode.Clamp,
                postWrapMode = WrapMode.Clamp
            };
    }
}
