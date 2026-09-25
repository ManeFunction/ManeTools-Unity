using System;
using System.Security.Cryptography;
using Mane.DotNet;
using Random = UnityEngine.Random;

namespace Mane.Unity
{
    /// <summary>
    /// <see cref="IRandom"/> implementation backed by <see cref="Random"/>.
    /// Each instance keeps its own <see cref="Random.State"/> so it does not
    /// disturb the global Unity RNG. The parameterless constructor seeds from
    /// a cryptographically strong source.
    /// Every instance shares one lock around the global RNG swap, so overlapping
    /// draws cannot interleave <see cref="Random.state"/>.
    /// </summary>
    public class UnityRandom : IRandom
    {
        private static readonly object _sync = new();
        private readonly int _seed;
        private Random.State _state;

        /// <inheritdoc />
        public int Seed => _seed;

        /// <summary>
        /// Creates a generator with the given seed.
        /// </summary>
        /// <param name="seed">Seed passed to <see cref="Random.InitState"/>.</param>
        public UnityRandom(int seed)
        {
            _seed = seed;
            InitFromSeed(seed);
        }

        /// <summary>
        /// Creates a generator seeded from <see cref="RandomNumberGenerator"/>.
        /// </summary>
        public UnityRandom()
        {
            Span<byte> bytes = stackalloc byte[4];
            RandomNumberGenerator.Fill(bytes);
            _seed = BitConverter.ToInt32(bytes);
            InitFromSeed(_seed);
        }

        /// <inheritdoc />
        public int Next(int min, int max)
        {
            if (min > max)
                throw new ArgumentOutOfRangeException(nameof(max));

            lock (_sync)
                return Draw(() => Random.Range(min, max));
        }

        /// <inheritdoc />
        public double Range01Double()
        {
            lock (_sync)
                return Draw(() => Random.Range(0, int.MaxValue) / (double)int.MaxValue);
        }

        /// <inheritdoc />
        public float Range01()
        {
            lock (_sync)
                return Draw(() => Random.Range(0, int.MaxValue) * (1f / int.MaxValue));
        }

        private T Draw<T>(Func<T> sample)
        {
            Random.State previous = Push();
            try
            {
                return sample();
            }
            finally
            {
                Pop(previous);
            }
        }

        private void InitFromSeed(int seed)
        {
            lock (_sync)
            {
                Random.State previous = Random.state;
                try
                {
                    Random.InitState(seed);
                    _state = Random.state;
                }
                finally
                {
                    Random.state = previous;
                }
            }
        }

        private Random.State Push()
        {
            Random.State previous = Random.state;
            Random.state = _state;
            return previous;
        }

        private void Pop(Random.State previous)
        {
            _state = Random.state;
            Random.state = previous;
        }
    }
}
