/*
 *  碰撞器也要对应增加更新叶子数据的功能
 */

using UnityEngine;


public class QuadtreeWithUpdateCollider : MonoBehaviour
{
    [SerializeField]
    float _radius;

    Transform _transform;
    QuadtreeWithUpdateLeaf<GameObject> _leaf;


    private void Awake()
    {
        _transform = transform;
        _leaf = new QuadtreeWithUpdateLeaf<GameObject>(gameObject, GetLeafPosition(), _radius);
    }
    Vector2 GetLeafPosition()
    {
        return new Vector2(_transform.position.x, _transform.position.y);
    }


    private void OnEnable()
    {
        UpdateLeaf();                               //存入叶子之前先更新一次叶子数据确保存入无误。实际上前两步也应该在存入前更新一次叶子数据，但前两步因为没有更新干脆把碰撞器当做固定的处理了
        QuadtreeWithUpdateObject.SetLeaf(_leaf);
    }


    private void Update()
    {
        UpdateLeaf();
    }
    void UpdateLeaf()
    {
        UpdateLeafPosition();
        UpdateLeafRadius();
    }
    void UpdateLeafPosition()
    {
        _leaf.position = GetLeafPosition();
    }
    void UpdateLeafRadius()
    {
        _leaf.radius = Mathf.Max(_transform.lossyScale.x, _transform.lossyScale.y) * _radius;       //注意是 lossyScale 不是localScale，lossyScale 是全局缩放，可以应对父物体缩放后碰撞器一起缩放的情况
        /*
         *  加了个应对缩放的功能，因为四叉树是不知道物体的缩放的。
         *  不过因为是圆形碰撞器所以不能变成椭圆碰撞区域，只能选缩放比较大的那个轴做基准。
         *  你要是喜欢的话也可以改成小的。
         */
    }


    private void OnDisable()
    {
        QuadtreeWithUpdateObject.RemoveLeaf(_leaf);
    }

    
    private void OnDrawGizmos()
    {
        if (!enabled) return;

        Gizmos.color = Color.green * 0.8f;

        MyGizmos.DrawCircle(transform.position, _radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y), 60);
    }
}
