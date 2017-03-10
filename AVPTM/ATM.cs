namespace AVPTM
{
    using System.Collections.Generic;
    using System.Linq;
    using MicrosoftResearch.Infer;
    using MicrosoftResearch.Infer.Distributions;
    using MicrosoftResearch.Infer.Maths;
    using MicrosoftResearch.Infer.Models;

    namespace AVPTM
    {
        /// <summary>
        /// Author topic model (ATM) implemented in Infer.NET.
        /// It keeps all messages in memory, and so scales poorly with respect to
        /// number of documents.
        /// An optional parameter to the constructor specifies whether to use the
        /// fast version of the model (which uses power plates to deal efficiently
        /// with repeated words in the document) or the slower version where
        /// each word is considered separately. The only advantage of the latter
        /// is that it supports an evidence calculation.
        /// of documents.
        /// </summary>
        public class ATM
        {
            /// <summary>
            /// Size of vocabulary
            /// </summary>
            public int SizeVocab { get; protected set; }

            /// <summary>
            /// Count of all authors
            /// </summary>
            public int AuthorCount { get; protected set; }

            /// <summary>
            /// Number of Topics
            /// </summary>
            public int NumTopics { get; protected set; }

            /// <summary>
            /// Sparsity specification for per-document distributions over topics
            /// </summary>
            public Sparsity ThetaSparsity { get; protected set; }

            /// <summary>
            /// Sparsity specification for per-topic distributions over words
            /// </summary>
            public Sparsity PhiSparsity { get; protected set; }

            /// <summary>
            /// Inference engine
            /// </summary>
            public InferenceEngine Engine { get; protected set; }

            /// <summary>
            /// Total number of documents (observed)
            /// </summary>
            protected Variable<int> NumDocuments;

            /// <summary>
            /// Number of words in each document (observed).
            /// For the fast version of the model, this is the number of unique words.
            /// </summary>
            protected VariableArray<int> NumWordsInDoc;

            /// <summary>
            /// Number of authors in each document (observed).
            /// </summary>
            protected VariableArray<int> NumAuthorsInDoc;

            /// <summary>
            /// Author indices in each document (observed)
            /// </summary>
            protected VariableArray<VariableArray<int>, int[][]> Authors;

            /// <summary>
            /// Word indices in each document (observed)
            /// For the fast version of the model, these are the unique word indices.
            /// </summary>
            protected VariableArray<VariableArray<int>, int[][]> Words;

            /// <summary>
            /// Counts of unique words in each document (observed).
            /// This is used for the fast version only
            /// </summary>
            protected VariableArray<VariableArray<double>, double[][]> WordCounts;

            /// <summary>
            /// Per document distribution over topics (to be inferred)
            /// </summary>
            protected VariableArray<Vector> Theta;

            /// <summary>
            /// Per topic distribution over words (to be inferred)
            /// </summary>
            protected VariableArray<Vector> Phi;

            /// <summary>
            /// Prior for <see cref="Theta"/>
            /// </summary>
            protected VariableArray<Dirichlet> ThetaPrior;

            /// <summary>
            /// Prior for <see cref="Phi"/>
            /// </summary>
            protected VariableArray<Dirichlet> PhiPrior;

            /// <summary>
            /// Model evidence
            /// </summary>
            protected Variable<bool> Evidence;

            /// <summary>
            /// Initialisation for breaking symmetry with respect to <see cref="Theta"/> (observed)
            /// </summary>
            protected Variable<IDistribution<Vector[]>> ThetaInit;

            /// <summary>
            /// Constructs an LDA model
            /// </summary>
            /// <param name="sizeVocab">Size of vocabulary</param>
            /// <param name="numTopics">Number of topics</param>
            /// <param name="authorCount">Count of all authors</param>
            public ATM(int sizeVocab, int numTopics, int authorCount)
            {
                SizeVocab = sizeVocab;
                NumTopics = numTopics;
                AuthorCount = authorCount;
                ThetaSparsity = Sparsity.Dense;
                PhiSparsity = Sparsity.ApproximateWithTolerance(1e-11); // Allow for round-off error

                // Surround model by a stochastic If block so that we can compute model evidence
                Evidence = Variable.Bernoulli(0.5).Named("Evidence");

                //---------------------------------------------
                // The model
                //---------------------------------------------
                NumDocuments = Variable.New<int>().Named("NumDocuments");
                var D = new Range(NumDocuments).Named("D");
                var W = new Range(SizeVocab).Named("W");
                var A = new Range(AuthorCount).Named("A");
                var T = new Range(NumTopics).Named("T");
                NumWordsInDoc = Variable.Array<int>(D).Named("NumWordsInDoc");
                var WInD = new Range(NumWordsInDoc[D]).Named("WInD");

                NumAuthorsInDoc = Variable.Array<int>(D).Named("NumAuthorsInDoc");
                var AInD = new Range(NumAuthorsInDoc[D]).Named("AInD");

                var evidenceBlock = Variable.If(Evidence);

                Theta = Variable.Array<Vector>(D);
                Theta.SetSparsity(ThetaSparsity);
                Theta.SetValueRange(T);
                ThetaPrior = Variable.Array<Dirichlet>(D).Named("ThetaPrior");
                Theta[D] = Variable<Vector>.Random(ThetaPrior[D]);
                Phi = Variable.Array<Vector>(T);
                Phi.SetSparsity(PhiSparsity);
                Phi.SetValueRange(W);
                PhiPrior = Variable.Array<Dirichlet>(T).Named("PhiPrior");
                Phi[T] = Variable<Vector>.Random(PhiPrior[T]);
                Words = Variable.Array(Variable.Array<int>(WInD), D).Named("Words");
                WordCounts = Variable.Array(Variable.Array<double>(WInD), D).Named("WordCounts");
                Authors = Variable.Array(Variable.Array<int>(AInD), D).Named("Authors");

                using (Variable.ForEach(D))
                {
                    using (Variable.ForEach(WInD))
                    {
                        using (Variable.Repeat(WordCounts[D][WInD]))
                        {
                            var topic = Variable.Discrete(Theta[D]).Named("topic");
                            using (Variable.Switch(topic))
                            {
                                Words[D][WInD] = Variable.Discrete(Phi[topic]);

                                // TODO: is this right?
                                using (Variable.Switch(Words[D][WInD]))
                                {
                                    Authors[D][AInD] = Variable.DiscreteUniform(AuthorCount);
                                }
                            }
                        }
                    }
                }

                evidenceBlock.CloseBlock();

                ThetaInit = Variable.New<IDistribution<Vector[]>>().Named("ThetaInit");
                Theta.InitialiseTo(ThetaInit);
                Engine = new InferenceEngine(new VariationalMessagePassing());
                Engine.Compiler.ShowWarnings = false;
                Engine.ModelName = "Author Topic Model";
            }

            /// <summary>
            /// Gets random initialisation for <see cref="Theta"/>. This initialises downward messages from <see cref="Theta"/>.
            /// The sole purpose is to break symmetry in the inference - it does not change the model.
            /// </summary>
            /// <param name="sparsity">The sparsity settings</param>
            /// <returns></returns>
            /// <remarks>This is implemented so as to support sparse initialisations</remarks>
            public static IDistribution<Vector[]> GetInitialisation(
                int numDocs, int numTopics, Sparsity sparsity)
            {
                Dirichlet[] initTheta = new Dirichlet[numDocs];
                double baseVal = 1.0 / numTopics;

                for (int i = 0; i < numDocs; i++)
                {
                    // Choose a random topic
                    Vector v = Vector.Zero(numTopics, sparsity);
                    int topic = Rand.Int(numTopics);
                    v[topic] = 1.0;
                    initTheta[i] = Dirichlet.PointMass(v);
                }
                return Distribution<Vector>.Array(initTheta);
            }

            /// <summary>
            /// Runs inference on the LDA model.
            /// <para>
            /// Words in documents are observed, topic distributions per document (<see cref="Theta"/>)
            /// and word distributions per topic (<see cref="Phi"/>) are inferred.
            /// </para>
            /// </summary>
            /// <param name="wordsInDoc">For each document, the unique word counts in the document</param>
            /// <param name="alpha">Hyper-parameter for <see cref="Theta"/></param>
            /// <param name="beta">Hyper-parameter for <see cref="Phi"/></param>
            /// <param name="postTheta">Posterior marginals for <see cref="Theta"/></param>
            /// <param name="postPhi">Posterior marginals for <see cref="Phi"/></param>
            /// <returns>Log evidence - can be used for model selection.</returns>
            public virtual double Infer(
                Dictionary<int, int>[] wordsInDoc,
                double alpha, double beta,
                out Dirichlet[] postTheta, out Dirichlet[] postPhi)
            {
                // Set up the observed values
                int numDocs = wordsInDoc.Length;
                NumDocuments.ObservedValue = numDocs;

                var numWordsInDoc = new int[numDocs];
                var wordIndices = new int[numDocs][];
                var wordCounts = new double[numDocs][];
                for (int i = 0; i < numDocs; i++)
                {
                    numWordsInDoc[i] = wordsInDoc[i].Count;
                    wordIndices[i] = wordsInDoc[i].Keys.ToArray();
                    ICollection<int> cnts = wordsInDoc[i].Values;
                    wordCounts[i] = new double[cnts.Count];
                    int k = 0;
                    foreach (int val in cnts)
                        wordCounts[i][k++] = val;
                }

                NumWordsInDoc.ObservedValue = numWordsInDoc;
                Words.ObservedValue = wordIndices;
                WordCounts.ObservedValue = wordCounts;
                ThetaInit.ObservedValue = GetInitialisation(numDocs, NumTopics, ThetaSparsity);
                ThetaPrior.ObservedValue = new Dirichlet[numDocs];
                for (int i = 0; i < numDocs; i++) ThetaPrior.ObservedValue[i] = Dirichlet.Symmetric(NumTopics, alpha);
                PhiPrior.ObservedValue = new Dirichlet[NumTopics];
                for (int i = 0; i < NumTopics; i++) PhiPrior.ObservedValue[i] = Dirichlet.Symmetric(SizeVocab, beta);
                Engine.OptimiseForVariables = new IVariable[] {Theta, Phi, Evidence};
                postTheta = Engine.Infer<Dirichlet[]>(Theta);
                postPhi = Engine.Infer<Dirichlet[]>(Phi);
                return Engine.Infer<Bernoulli>(Evidence).LogOdds;
            }
        }
    }
}