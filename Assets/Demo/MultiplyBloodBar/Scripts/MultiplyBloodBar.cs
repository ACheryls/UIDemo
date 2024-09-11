using UnityEngine;
using System.Collections;
using UnityEngine.UI;

//如果只有一条血，那么一条血就是所有的血量
//如果有多条血，那么一条血就设定为一个固定值
public class MultiplyBloodBar : MonoBehaviour
{
    public Image nowBar;                            //当前血条
    public Image middleBar;                         //过渡血条
    public Image nextBar;                           //下一血条
    public Text countText;                          //剩下的血条数text

    private int count;                              //剩下的血条数(不包括当前血量)
    private float nowBlood;                         //在一条血中的当前血量
    private float oneBarBlood;             //一条血的容量

    private int colorIndex = 0;
    public Color[] colors;                          //血条的颜色，注意Alpha值，默认为0

    private float slowSpeed = 0.1f;                 //受到重伤时( >oneBarBlood)或者处于加血状态，当前血条的流动速度  
    private float quickSpeed = 1f;                  //受到轻伤时( <oneBarBlood)，当前血条的流动速度  
    private float speed;                            //当前血条采用的速度  
    private float middleBarSpeed = 0.1f;            //过渡血条的流动速度  

    private float nowTargetValue;                   //当前血条移动的目标点 
    private float middleTargetValue;                //过渡血条移动的目标点 
    private bool isBloodMove = false;               //控制血条的移动  

    void Update()
    {
        MoveNowBar();                               //当前血条的流动 
        MoveMiddleBar();                            //过渡血条的流动  
        if (Input.GetMouseButtonDown(0))
        {
            ChangeBlood(-3000);
        }
        else if (Input.GetMouseButtonDown(1))
        {
            ChangeBlood(-15000);
        }
    }
    private void Awake()
    {

    }
    private void Start()
    {
        InitBlood(100000,10000);
    }
    /// <summary>  
    /// 传入总血量，初始化血条  
    /// </summary>  
    /// <param name="number"></param>  
    public void InitBlood(float maxHp,float oneBarBloodNum)
    {
        oneBarBlood = oneBarBloodNum;
        count = (int)(maxHp / oneBarBlood);//血条数
        nowBlood = maxHp % oneBarBlood;
        if (nowBlood == 0)  //满血
        {
            nowBlood = oneBarBlood;
            count--;
        }

        colorIndex = count % colors.Length;
        nowBar.color = colors[colorIndex];
        nowBar.fillAmount = nowBlood / oneBarBlood;

        if (count != 0)//设置下一条血条的颜色和显示状态
        {
            int nextColorIndex = (colorIndex - 1 + colors.Length) % colors.Length;
            nextBar.color = colors[nextColorIndex];
            nextBar.gameObject.SetActive(true);
        }
        else
        {
            nextBar.gameObject.SetActive(false);
        }

        middleBar.gameObject.SetActive(false);

        countText.text = count + "";
    }

    /// <summary>  
    /// 血量变化，并根据伤害判断是否使用过渡血条  
    /// </summary>  
    /// <param name="number"></param>  
    public void ChangeBlood(float number)
    {
        nowBlood += number;
        nowTargetValue = nowBlood / oneBarBlood;
        isBloodMove = true;

        if ((number < 0) && (Mathf.Abs(number) <= oneBarBlood))//处于受伤状态并且伤害量较低时  
        {
            speed = quickSpeed;
            middleBar.gameObject.SetActive(true);
            middleBar.transform.SetSiblingIndex(nextBar.transform.GetSiblingIndex() + 1);
            middleBar.fillAmount = nowBar.fillAmount;
            middleTargetValue = nowTargetValue;
        }
        else//处于受伤状态并且伤害量较大时，或者处于加血状态  
        {
            speed = slowSpeed;
            middleBar.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 普通血条的流动 
    /// </summary>
    void MoveNowBar()
    {
        if (!isBloodMove) return;

        nowBar.fillAmount = Mathf.Lerp(nowBar.fillAmount, nowTargetValue, speed);

        if (Mathf.Abs(nowBar.fillAmount - nowTargetValue) <= 0.01f)//到达目标点  
            isBloodMove = false;

        if (count == 0)
            nextBar.gameObject.SetActive(false);
        else
            nextBar.gameObject.SetActive(true);

        if (nowBar.fillAmount >= nowTargetValue)
            SubBlood();
        else
            AddBlood();
    }

    /// <summary>
    /// 过渡血条的流动  
    /// </summary>
    void MoveMiddleBar()
    {
        //受到轻伤时( <oneBarBlood)，才会出现过渡血条
        if (speed == quickSpeed)
        {
            middleBar.fillAmount = Mathf.Lerp(middleBar.fillAmount, middleTargetValue, middleBarSpeed);
            if (Mathf.Abs(middleBar.fillAmount - 0) < 0.01f)
            {
                middleBar.transform.SetSiblingIndex(nextBar.transform.GetSiblingIndex() + 1);
                middleBar.fillAmount = 1;
                middleTargetValue++;
            }
        }
    }

    void AddBlood()
    {
        float subValue = Mathf.Abs(nowBar.fillAmount - 1);
        if (subValue <= 0.0f)//到达1  
        {
            count++;
            countText.text = count.ToString();

            nowBar.fillAmount = 0;
            nowTargetValue -= 1;
            nowBlood -= oneBarBlood;

            nextBar.color = colors[colorIndex];

            colorIndex++;
            colorIndex %= colors.Length;
            nowBar.color = colors[colorIndex];
        }
    }

    void SubBlood()
    {
        float subValue = Mathf.Abs(nowBar.fillAmount - 0);
        if (subValue <= 0.0f)//到达0  
        {
            //当前血条已经流动完，将过渡血条放置最前
            middleBar.transform.SetSiblingIndex(nextBar.transform.GetSiblingIndex() + 2);

            if (count <= 0)
            {
                middleBar.gameObject.SetActive(false);
                Destroy(this);
                return;
            };
            count--;
            countText.text = count.ToString();

            nowBar.fillAmount = 1;
            nowTargetValue += 1;
            nowBlood += oneBarBlood;

            colorIndex--;
            colorIndex += colors.Length;
            colorIndex %= colors.Length;
            nowBar.color = colors[colorIndex];

            int nextColorIndex = colorIndex - 1 + colors.Length;
            nextColorIndex %= colors.Length;
            nextBar.color = colors[nextColorIndex];
        }
    }
    /// <summary>
    /// 根据当前血量和最大血量百分比来设置进度
    /// </summary>
    /// <param name="currentHealth">当前血量</param>
    /// <param name="maxHealth">最大血量</param>
    public void SetProgressByPercentage(float currentHealth, float maxHealth)
    {
        // 计算当前血量占总血量的百分比
        float percentage = currentHealth / maxHealth;

        // 计算现在的血条应该在哪一条血上
        count = (int)(percentage * maxHealth / oneBarBlood);
        nowBlood = (percentage * maxHealth) % oneBarBlood;

        // 更新当前血条的显示
        nowTargetValue = nowBlood / oneBarBlood;
        nowBar.fillAmount = nowTargetValue;

        // 更新颜色和下一个血条的状态
        colorIndex = count % colors.Length;
        nowBar.color = colors[colorIndex];

        if (count != 0)
        {
            int nextColorIndex = (colorIndex - 1 + colors.Length) % colors.Length;
            nextBar.color = colors[nextColorIndex];
            nextBar.gameObject.SetActive(true);
        }
        else
        {
            nextBar.gameObject.SetActive(false);
        }

        countText.text = count.ToString();
    }


}