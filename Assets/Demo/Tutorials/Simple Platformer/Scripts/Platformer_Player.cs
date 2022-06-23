using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ASL;

public class Platformer_Player : MonoBehaviour
{
    public float MovementSpeed = 3f;
    public float FallVelocity = 0.3f;
    public float JumpVelocity = 10f;
    public Vector3 RespawnPoint = new Vector3(-5, -2, 0);
    public Text CoinCount;
    public Text WinText;

    Vector3 velocity = Vector3.zero;
    bool topCollision = false;
    bool bottomCollision = false;
    bool leftCollision = false;
    bool rightCollision = false;
    bool jumpRecharged = true;
    int coinsCollected = 0;
    float floor, leftWall, rightWall; //these need to reset on triggerExit

    ASLObject m_ASLObject;
    ASL_UserObject m_UserObject;

    // Start is called before the first frame update
    void Start()
    {
        m_ASLObject = GetComponent<ASLObject>();
        Debug.Assert(m_ASLObject != null);
        CoinCount = FindObjectOfType<Platformer_GameManager>().CoinCount;
        WinText = FindObjectOfType<Platformer_GameManager>().WinText;
        if (Camera.main.GetComponent<Platformer_CameraMove>() != null)
        {
            Camera.main.GetComponent<Platformer_CameraMove>().SetUpCamera(this);
        }

        m_UserObject = GetComponent<ASL_UserObject>();
        Debug.Assert(m_UserObject != null);

        m_ASLObject._LocallySetFloatCallback(floatFunction);
    }

    private void Update()
    {
        if (m_UserObject.IsOwner(ASL.GameLiftManager.GetInstance().m_PeerId) && Input.GetKeyDown(KeyCode.Space) && jumpRecharged)
        {
            jumpRecharged = false;
            velocity.y += JumpVelocity;
            Debug.Log("JUMP");
        }
        if (true)//m_UserObject.IsOwner(ASL.GameLiftManager.GetInstance().m_PeerId))
        {
            bool hitGround = false;
            bool hitLeft = false;
            bool hitRight = false;

            float horizontalInput = Input.GetAxis("Horizontal");
            velocity.x = horizontalInput * MovementSpeed;
            if (topCollision && velocity.y < 0)
            {
                velocity.y = 0;
                hitGround = true;
            }
            else if (!topCollision)
            {
                velocity.y -= FallVelocity;
            }
            if (bottomCollision && velocity.y > 0)
            {
                velocity.y = 0;
            }
            if (leftCollision && velocity.x > 0)
            {
                velocity.x = 0;
                hitLeft = true;
            }
            if (rightCollision && velocity.x < 0)
            {
                velocity.x = 0f;
                hitRight = true;
            }
            Vector3 m_AdditiveMovementAmount = velocity * Time.deltaTime;
            if (topCollision && hitGround)
            {
                Vector3 endPos = transform.position + m_AdditiveMovementAmount;
                endPos.y = floor;
                m_AdditiveMovementAmount = endPos - transform.position;
            }
            if (leftCollision && hitLeft)
            {
                Vector3 endPos = transform.position + m_AdditiveMovementAmount;
                endPos.x = leftWall;
                m_AdditiveMovementAmount = endPos - transform.position;
            }
            if (rightCollision && hitRight)
            {
                Vector3 endPos = transform.position + m_AdditiveMovementAmount;
                endPos.x = rightWall;
                m_AdditiveMovementAmount = endPos - transform.position;
            }
            m_UserObject.IncrementWorldPosition(m_AdditiveMovementAmount);
        }
    }

    public void PlatformCollisionEnter(Platformer_Collider.CollisionSide side, float collisionPoint)
    {
        float[] _f = new float[4];
        _f[0] = 2;
        switch (side)
        {
            case Platformer_Collider.CollisionSide.top:
                _f[1] = 0;
                break;
            case Platformer_Collider.CollisionSide.bottom:
                _f[1] = 1;
                break;
            case Platformer_Collider.CollisionSide.left:
                _f[1] = 2;
                break;
            case Platformer_Collider.CollisionSide.right:
                _f[1] = 3;
                break;
        }
        _f[2] = 1;
        _f[3] = collisionPoint;

        m_ASLObject.SendAndSetClaim(() =>
        {
            m_ASLObject.SendFloatArray(_f);
        });
    }

    public void PlatformCollisionExit(Platformer_Collider.CollisionSide side)
    {
        float[] _f = new float[3];
        _f[0] = 2;
        switch (side)
        {
            case Platformer_Collider.CollisionSide.top:
                _f[1] = 0;
                break;
            case Platformer_Collider.CollisionSide.bottom:
                _f[1] = 1;
                break;
            case Platformer_Collider.CollisionSide.left:
                _f[1] = 2;
                break;
            case Platformer_Collider.CollisionSide.right:
                _f[1] = 3;
                break;
            default:
                Debug.LogError("side was not properly defined");
                break;
        }
        _f[2] = 0;
        m_ASLObject.SendAndSetClaim(() =>
        {
            m_ASLObject.SendFloatArray(_f);
        });
    }

    public void StayOnPlatform(Transform platform)
    {
        transform.parent = platform;
    }

    public void ExitPlatform()
    {
        transform.parent = null;
    }

    public void KillEnemy()
    {
        //enemy.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
        //{
        //    enemy.GetComponent<ASL.ASLObject>().DeleteObject();
        //});
        velocity.y = JumpVelocity * 0.5f;
    }

    public void ResetPlayer()
    {
        m_ASLObject.SendAndSetClaim(() =>
        {
            m_ASLObject.SendAndSetWorldPosition(RespawnPoint);
        });
    }

    public void CollectCoin(GameObject coin)
    {
        coinsCollected++;
        CoinCount.text = "Coins Collected: " + coinsCollected;
        coin.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
        {
            coin.GetComponent<ASL.ASLObject>().DeleteObject();
        });
    }

    public void EnterWinZone()
    {
        WinText.gameObject.SetActive(true);
        WinText.text = "Player 1 Wins!!!";
    }

    //_f{opporation, side, enter/exit, collisionPoint}
    public void floatFunction(string _id, float[] _f)
    {
        Debug.Log("player float function");
        if (_f[0] == 1)
        {
            m_UserObject.SetOwner(_id, new float[2] { _f[0], _f[1]});
        }
        else if (_f[0] == 2)
        {
            Debug.Log("floatFunction Called");
            float collisionSide = _f[1];
            bool collisionEnter;
            float collisionPoint = 0;
            if (_f[2] == 0)
            {
                collisionEnter = false;
            }
            else
            {
                collisionEnter = true;
                collisionPoint = _f[3];
            }
            switch (collisionSide)
            {
                case 0:
                    //top
                    topCollision = collisionEnter;
                    if (topCollision)
                    {
                        jumpRecharged = true;
                        floor = collisionPoint + transform.localScale.y / 2;
                        Debug.Log("Hit ground");
                    }
                    break;
                case 1:
                    //bottom
                    bottomCollision = collisionEnter;
                    break;
                case 2:
                    //left
                    leftCollision = collisionEnter;
                    if (leftCollision)
                    {
                        leftWall = collisionPoint - transform.localScale.x / 2;
                    }
                    break;
                case 3:
                    //right
                    rightCollision = collisionEnter;
                    if (rightCollision)
                    {
                        rightWall = collisionPoint + transform.localScale.x / 2;
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
