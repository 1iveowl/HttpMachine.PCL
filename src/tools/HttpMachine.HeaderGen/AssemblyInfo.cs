using System.Runtime.CompilerServices;

// The cacheability test needs TrackingNames to pull each pipeline stage out of TrackedSteps.
// Exposed this way rather than by making the type public: the tracking names are an implementation
// detail of the pipeline, not something a consumer of the generator should see.
[assembly: InternalsVisibleTo("HeaderGen.Tests")]
