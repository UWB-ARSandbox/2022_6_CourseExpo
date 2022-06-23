using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngineInternal;

// If you run into this error:
// 'Android Studio - ARCore - E/DynamiteClient: Failed to load native library [packageName=com.google.ar.core,libraryName=arcore_c] from remote package:'
// it is possible that the phone you are using has an incompatiable processor. See this
// (https://stackoverflow.com/questions/61392883/android-studio-arcore-e-dynamiteclient-failed-to-load-native-library-packa)
// for more information. It is recommended that you use a google phone, such as a Google Pixle XL.

namespace ASL
{
    /// <summary>
    /// Allows video streaming between a single Android device and a single Desktop. FPS and resolution can be adjusted.
    /// By default, streaming must be enabled by the Android device to protect the user's privacy.
    /// ALLOW_REMOTE_CAMERA_ACTIVATION will allow camera settings to be adjusted from the desktop. Only enable this flag
    /// for debug purposes, or if both users have been informed and consented.
    ///
    /// The object contains VideoStream should also have an ASLObject!
    /// 
    /// </summary>
    public class VideoStream : MonoBehaviour
    {
        #region Member Variables

        private static readonly bool ALLOW_REMOTE_CAMERA_ACTIVATION = false;

        private bool isMobileDevice;
        public bool IsStreaming { get; private set; }
        private bool streamingCoroutineActive;

        public float FPS { get; set; } = 10.0f;
        public int ImageWidth { get; set; } = Screen.width / 6;
        public int ImageHeight { get; set; } = Screen.height / 6;

        //Android garbage collection cannot keep up with the memory utilized by streaming textures
        //Using too much memory will cause sudden crashes
        //We call UnloadUnusedAssets on a regular basis to ensure that memory usage is not too high
        //Make GCCollectFreq more frequent if crashes are occuring due to memory usage
        private int gcFrameCount = 0; //Garbage Collector Frame Count
        private int GCCollectFreq = 50; //Frames until forced garbage collection

        private ASLObject aslObject;

        public Action<GameObject, Texture2D> OnFrameUpdate { get; set; }

        #endregion


        #region Unity Functions

        /// <summary>
        /// 
        /// </summary>
        private void Awake()
        {
            isMobileDevice = Application.isMobilePlatform;

            IsStreaming = false;
            streamingCoroutineActive = false;

            Debug.Assert(gameObject.GetComponent<ASLObject>() != null,
                "Video Stream Requires an ASLObject to function!");

            aslObject = gameObject.GetComponent<ASLObject>();

            aslObject._LocallySetFloatCallback(OnFloatsReceived);
        }

        #endregion


        #region Public Functions

        /// <summary>
        /// Start video streaming. Can be called from either client if ALLOW_REMOTE_CAMERA_ACTIVATION is true.
        /// </summary>
        /// <returns>
        /// true if the stream was started. False if it could not be started or was already streaming
        /// </returns>
        public bool StartStream()
        {
            if (!isMobileDevice && !ALLOW_REMOTE_CAMERA_ACTIVATION)
            {
                return false;
            }

            if (IsStreaming)
            {
                return false;
            }

            IsStreaming = true;

            SynchronizePeers();

            if (isMobileDevice)
            {
                StartCoroutine(StreamVideo());
            }

            return true;
        }

        /// <summary>
        /// Stop video streaming. Can be called from either client if ALLOW_REMOTE_CAMERA_ACTIVATION is true.
        /// </summary>
        /// <returns>
        /// true if the stream was stopped. False if it could not be stopped or was not streaming
        /// </returns>
        public bool StopStream()
        {
            if (!isMobileDevice && !ALLOW_REMOTE_CAMERA_ACTIVATION)
            {
                return false;
            }

            if (!IsStreaming)
            {
                return false;
            }

            IsStreaming = false;

            SynchronizePeers();

            return true;
        }

        /// <summary>
        /// Send a single frame. Can be called from either client if ALLOW_REMOTE_CAMERA_ACTIVATION is true.
        /// </summary>
        /// <returns>
        /// true if the frame was sent. False if it could not be sent.
        /// </returns>
        public bool SendFrame()
        {
            if (!isMobileDevice && !ALLOW_REMOTE_CAMERA_ACTIVATION)
            {
                return false;
            }

            if (IsStreaming)
            {
                return false;
            }

            if (!isMobileDevice) //PC requests Mobile device to send a frame
            {
                RequestFrame();
            }
            else // Mobile device simply sends a frame
            {
                SendScreenCapture();
            }

            return true;
        }

        #endregion


        #region Internal

        //Call the user-defined callback when a frame is delivered
        private static void OnFrameDelivered(GameObject gObject, Texture2D texture2D)
        {
            if (Application.isMobilePlatform) //Mobile devices don't do anything since they sent it
            {
                return;
            }

            VideoStream videoStream = gObject.GetComponent<VideoStream>();
            videoStream.OnFrameUpdate?.Invoke(gObject, texture2D);
        }


        //Main coroutine that the mobile device runs
        //streamingCoroutineActive - instance variable that shows if this Coroutine is alive
        //IsStreaming - static instance variable that shows if this Coroutine _should_ be alive
        private IEnumerator StreamVideo()
        {
            streamingCoroutineActive = true;

            while (IsStreaming)
            {
                try
                {
                    SendScreenCapture();

                    if (gcFrameCount > GCCollectFreq)
                    {
                        ForceGarbageCollection();

                        gcFrameCount = 0;
                    }

                    gcFrameCount++;
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }

                yield return new WaitForSeconds(1 / FPS);
            }

            streamingCoroutineActive = false;
        }

        //Captures the screen and sends it as a texture through ASL
        private void SendScreenCapture()
        {
            Texture2D texture2D = ScreenCapture.CaptureScreenshotAsTexture();

            TextureScaler.scale(texture2D, ImageWidth, ImageHeight);

            aslObject.SendAndSetTexture2D(texture2D, OnFrameDelivered, false);

            Destroy(texture2D);
        }

        //Forces the device to clean up memory - prevents crashes on Android
        private void ForceGarbageCollection()
        {
            Resources.UnloadUnusedAssets();
        }

        //Sends all parameters to all peers via SendFloatArray()
        private void SynchronizePeers()
        {
            float[] floatArray = { FPS, ImageWidth, ImageHeight, GCCollectFreq, -1.0f };

            if (IsStreaming)
            {
                floatArray[4] = 1;
            }
            else
            {
                floatArray[4] = 0;
            }

            aslObject.SendAndSetClaim(() =>
            {
                aslObject.SendFloatArray(floatArray);
            });
        }

        //Requests peers to send a single screen capture
        private void RequestFrame()
        {
            float[] floatArray = { -1.0f };

            aslObject.SendAndSetClaim(() =>
            {
                aslObject.SendFloatArray(floatArray);
            });
        }

        //Starts the streaming coroutine if the device can stream
        private void StartStreamingCoroutine()
        {
            if (isMobileDevice && IsStreaming && !streamingCoroutineActive)
            {
                StartCoroutine(StreamVideo());
            }
        }

        //Callback when a float array is sent
        private static void OnFloatsReceived(string _id, float[] floatArray)
        {
            ASLObject sender;
            VideoStream videoStream;

            if (ASL.ASLHelper.m_ASLObjects.TryGetValue(_id, out sender))
            {
                videoStream = sender.GetComponent<VideoStream>();

                if (videoStream == null)
                {
                    Debug.LogError("VideoStream - OnFloatsReceived: ASL Object does not have VideoStream");

                    return;
                }
            }
            else
            {
                Debug.LogError("VideoStream - OnFloatsReceived: Could not find sender");
                return;
            }


            if (floatArray.Length == 1 && floatArray[0] == -1)
            {
                if (Application.isMobilePlatform)
                    videoStream.SendScreenCapture();

                return;
            }

            videoStream.FPS = floatArray[0];
            videoStream.ImageWidth = (int)floatArray[1];
            videoStream.ImageHeight = (int)floatArray[2];
            videoStream.GCCollectFreq = (int)floatArray[3];

            if (floatArray[4] == 1.0f)
            {
                videoStream.IsStreaming = true;
                videoStream.StartStreamingCoroutine();
            }
            else
            {
                videoStream.IsStreaming = false;
            }
        }

        #endregion
    }
}