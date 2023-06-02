using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MagesnShadows.Manager
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private GameObject networkHudCanvas;
        [SerializeField] private NetworkHudCanvases p;
        [SerializeField] private GameObject ui_Canvas;


        //`6    [SerializeField] private GameObject s;//, c;
        [SerializeField] private GameObject s, c;
        [SerializeField] private bool isShown, connect;

        // Start is called before the first frame update
        void Start()
        {
            if(networkHudCanvas == null)
            {
                networkHudCanvas = GameObject.Find("NetworkHudCanvas");
                p = networkHudCanvas.GetComponent<NetworkHudCanvases>();

                var sl = networkHudCanvas.transform.Find("Selector").gameObject;
                //s = sl.transform.Find("Server").gameObject;
                //c = sl.transform.Find("Client").gameObject;

                s = networkHudCanvas.transform.Find("Server").gameObject;
                c = networkHudCanvas.transform.Find("Client").gameObject;

                //s = networkHudCanvas.transform.Find("Selector").gameObject;
            }
            if(ui_Canvas == null)
            {
                ui_Canvas = GameObject.Find("UI_Canvas");
            }

        }

        // Update is called once per frame
        void Update()
        {

            
            if (Input.GetKeyDown(KeyCode.T))
            {
                isShown = !isShown;
                s.gameObject.SetActive(isShown);
                c.gameObject.SetActive(isShown);


            }
            if (Input.GetKeyDown(KeyCode.O))
            {
                
                if (connect)
                {
                    connect = !connect;
                    p.OnClick_Server();
                    p.OnClick_Client();
                }
                else
                {
                    p.OnClick_Client();
                    p.OnClick_Server();
                    
                }
                
            
            }
        }
    }
}
