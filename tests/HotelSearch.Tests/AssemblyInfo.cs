using Xunit;

// Integration fixtures each start PostgreSQL via Testcontainers; run collections sequentially
// to avoid cross-fixture database interference when container reuse is enabled locally.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
