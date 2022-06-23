using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ASL;

namespace SimpleDemos
{
    /// <summary>
    /// A simple VideoStream demo in which the mobile user can toggle the stream with a button, and the Desktop user
    /// can view the stream.
    /// </summary>
    public class VideoStream_Example : MonoBehaviour
    {
        public RawImage m_VideoDisplay;
        public Button m_ToggleStream;

        private VideoStream m_VideoStream;

        void Start()
        {
            m_VideoStream = GetComponent<VideoStream>();

            //On the Desktop, call onFrameUpdate whenever we receive a frame to update the image
            if (!Application.isMobilePlatform)
                m_VideoStream.OnFrameUpdate = (GameObject gObject, Texture2D texture2D) =>
                {
                    m_VideoDisplay.texture = texture2D;
                };

            //On the mobile device, toggle streaming when the button is pressed
            m_ToggleStream.onClick.AddListener(() =>
            {
                if (m_VideoStream.IsStreaming)
                {
                    m_VideoStream.StopStream();
                }
                else
                {
                    m_VideoStream.StartStream();
                }
            });
        }
    }
}
