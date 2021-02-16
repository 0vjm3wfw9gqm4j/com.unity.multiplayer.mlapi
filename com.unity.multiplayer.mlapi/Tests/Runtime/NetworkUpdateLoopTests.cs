using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;

namespace MLAPI.RuntimeTests
{
    public class NetworkUpdateLoopTests
    {
        private struct NetworkUpdateCallbacks
        {
            public Action OnInitialization;
            public Action OnEarlyUpdate;
            public Action OnFixedUpdate;
            public Action OnPreUpdate;
            public Action OnUpdate;
            public Action OnPreLateUpdate;
            public Action OnPostLateUpdate;
        }

        private class MyPlainScript : IDisposable, INetworkUpdateSystem
        {
            public NetworkUpdateCallbacks UpdateCallbacks;

            public void Initialize()
            {
                this.RegisterNetworkUpdate(NetworkUpdateStage.EarlyUpdate);
                this.RegisterNetworkUpdate(NetworkUpdateStage.PreLateUpdate);
            }

            public void NetworkUpdate(NetworkUpdateStage updateStage)
            {
                switch (updateStage)
                {
                    case NetworkUpdateStage.Initialization:
                        UpdateCallbacks.OnInitialization();
                        break;
                    case NetworkUpdateStage.EarlyUpdate:
                        UpdateCallbacks.OnEarlyUpdate();
                        break;
                    case NetworkUpdateStage.FixedUpdate:
                        UpdateCallbacks.OnFixedUpdate();
                        break;
                    case NetworkUpdateStage.PreUpdate:
                        UpdateCallbacks.OnPreUpdate();
                        break;
                    case NetworkUpdateStage.Update:
                        UpdateCallbacks.OnUpdate();
                        break;
                    case NetworkUpdateStage.PreLateUpdate:
                        UpdateCallbacks.OnPreLateUpdate();
                        break;
                    case NetworkUpdateStage.PostLateUpdate:
                        UpdateCallbacks.OnPostLateUpdate();
                        break;
                }
            }

            public void Dispose()
            {
                this.UnregisterAllNetworkUpdates();
            }
        }

        [UnityTest]
        public IEnumerator UpdateStagesPlain()
        {
            const int kNetInitializationIndex = 0;
            const int kNetEarlyUpdateIndex = 1;
            const int kNetFixedUpdateIndex = 2;
            const int kNetPreUpdateIndex = 3;
            const int kNetUpdateIndex = 4;
            const int kNetPreLateUpdateIndex = 5;
            const int kNetPostLateUpdateIndex = 6;
            int[] netUpdates = new int[7];

            yield return new WaitForEndOfFrame();
            bool isTesting = false;

            using (var plainScript = new MyPlainScript())
            {
                plainScript.UpdateCallbacks = new NetworkUpdateCallbacks
                {
                    OnInitialization = () =>
                    {
                        if (isTesting)
                        {
                            netUpdates[kNetInitializationIndex]++;
                        }
                    },
                    OnEarlyUpdate = () =>
                    {
                        if (isTesting)
                        {
                            netUpdates[kNetEarlyUpdateIndex]++;
                        }
                    },
                    OnFixedUpdate = () =>
                    {
                        if (isTesting)
                        {
                            netUpdates[kNetFixedUpdateIndex]++;
                        }
                    },
                    OnPreUpdate = () =>
                    {
                        if (isTesting)
                        {
                            netUpdates[kNetPreUpdateIndex]++;
                        }
                    },
                    OnUpdate = () =>
                    {
                        if (isTesting)
                        {
                            netUpdates[kNetUpdateIndex]++;
                        }
                    },
                    OnPreLateUpdate = () =>
                    {
                        if (isTesting)
                        {
                            netUpdates[kNetPreLateUpdateIndex]++;
                        }
                    },
                    OnPostLateUpdate = () =>
                    {
                        if (isTesting)
                        {
                            netUpdates[kNetPostLateUpdateIndex]++;
                        }
                    }
                };

                plainScript.Initialize();
                yield return new WaitForEndOfFrame();
                isTesting = true;

                for (int i = 0; i < 16; i++)
                {
                    Assert.AreEqual(0, netUpdates[kNetInitializationIndex]);
                    Assert.AreEqual(i, netUpdates[kNetEarlyUpdateIndex]);
                    Assert.AreEqual(0, netUpdates[kNetFixedUpdateIndex]);
                    Assert.AreEqual(0, netUpdates[kNetPreUpdateIndex]);
                    Assert.AreEqual(0, netUpdates[kNetUpdateIndex]);
                    Assert.AreEqual(i, netUpdates[kNetPreLateUpdateIndex]);
                    Assert.AreEqual(0, netUpdates[kNetPostLateUpdateIndex]);
                    yield return new WaitForEndOfFrame();
                }
            }
        }

        private struct MonoBehaviourCallbacks
        {
            public Action OnFixedUpdate;
            public Action OnUpdate;
            public Action OnLateUpdate;
        }

        private class MyGameScript : MonoBehaviour, INetworkUpdateSystem
        {
            public NetworkUpdateCallbacks UpdateCallbacks;
            public MonoBehaviourCallbacks BehaviourCallbacks;

            private void Awake()
            {
                this.RegisterNetworkUpdate(NetworkUpdateStage.FixedUpdate);
                this.RegisterNetworkUpdate(NetworkUpdateStage.PreUpdate);
                this.RegisterNetworkUpdate(NetworkUpdateStage.PreLateUpdate);
            }

            public void NetworkUpdate(NetworkUpdateStage updateStage)
            {
                switch (updateStage)
                {
                    case NetworkUpdateStage.FixedUpdate:
                        UpdateCallbacks.OnFixedUpdate();
                        break;
                    case NetworkUpdateStage.PreUpdate:
                        UpdateCallbacks.OnPreUpdate();
                        break;
                    case NetworkUpdateStage.PreLateUpdate:
                        UpdateCallbacks.OnPreLateUpdate();
                        break;
                }
            }

            private void FixedUpdate()
            {
                BehaviourCallbacks.OnFixedUpdate();
            }

            private void Update()
            {
                BehaviourCallbacks.OnUpdate();
            }

            private void LateUpdate()
            {
                BehaviourCallbacks.OnLateUpdate();
            }

            private void OnDestroy()
            {
                this.UnregisterAllNetworkUpdates();
            }
        }

        [UnityTest]
        public IEnumerator UpdateStagesMixed()
        {
            const int kNetFixedUpdateIndex = 0;
            const int kNetPreUpdateIndex = 1;
            const int kNetPreLateUpdateIndex = 2;
            int[] netUpdates = new int[3];
            const int kMonoFixedUpdateIndex = 0;
            const int kMonoUpdateIndex = 1;
            const int kMonoLateUpdateIndex = 2;
            int[] monoUpdates = new int[3];

            yield return new WaitForEndOfFrame();
            bool isTesting = false;

            {
                var gameObject = new GameObject($"{nameof(NetworkUpdateLoopTests)}.{nameof(UpdateStagesMixed)} (Dummy)");
                var gameScript = gameObject.AddComponent<MyGameScript>();
                gameScript.UpdateCallbacks = new NetworkUpdateCallbacks
                {
                    OnFixedUpdate = () =>
                    {
                        if (isTesting)
                        {
                            netUpdates[kNetFixedUpdateIndex]++;
                            Assert.AreEqual(monoUpdates[kMonoFixedUpdateIndex] + 1, netUpdates[kNetFixedUpdateIndex]);
                        }
                    },
                    OnPreUpdate = () =>
                    {
                        if (isTesting)
                        {
                            netUpdates[kNetPreUpdateIndex]++;
                            Assert.AreEqual(monoUpdates[kMonoUpdateIndex] + 1, netUpdates[kNetPreUpdateIndex]);
                        }
                    },
                    OnPreLateUpdate = () =>
                    {
                        if (isTesting)
                        {
                            netUpdates[kNetPreLateUpdateIndex]++;
                            Assert.AreEqual(monoUpdates[kMonoLateUpdateIndex] + 1, netUpdates[kNetPreLateUpdateIndex]);
                        }
                    }
                };
                gameScript.BehaviourCallbacks = new MonoBehaviourCallbacks
                {
                    OnFixedUpdate = () =>
                    {
                        if (isTesting)
                        {
                            monoUpdates[kMonoFixedUpdateIndex]++;
                            Assert.AreEqual(netUpdates[kNetFixedUpdateIndex], monoUpdates[kMonoFixedUpdateIndex]);
                        }
                    },
                    OnUpdate = () =>
                    {
                        if (isTesting)
                        {
                            monoUpdates[kMonoUpdateIndex]++;
                            Assert.AreEqual(netUpdates[kNetPreUpdateIndex], monoUpdates[kMonoUpdateIndex]);
                        }
                    },
                    OnLateUpdate = () =>
                    {
                        if (isTesting)
                        {
                            monoUpdates[kMonoLateUpdateIndex]++;
                            Assert.AreEqual(netUpdates[kNetPreLateUpdateIndex], monoUpdates[kMonoLateUpdateIndex]);
                        }
                    }
                };

                yield return new WaitForEndOfFrame();
                isTesting = true;

                for (int i = 0; i < 16; i++)
                {
                    Assert.AreEqual(i, netUpdates[kNetPreUpdateIndex]);
                    Assert.AreEqual(netUpdates[kNetPreUpdateIndex], monoUpdates[kMonoUpdateIndex]);
                    yield return new WaitForEndOfFrame();
                }

                GameObject.DestroyImmediate(gameObject);
            }
        }
    }
}