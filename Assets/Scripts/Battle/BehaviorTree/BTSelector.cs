using System.Collections.Generic;
//여러 선택지 중 실행 가능한 행동을 찾는 노드
public class BTSelector : BTNode
{
    private readonly List<BTNode> children;

    public BTSelector(params BTNode[] children)
    {
        this.children = new List<BTNode>(children);
    }
    public override BTStatus Run()
    {
        for (int i = 0; i < children.Count; i++)
        {
            BTStatus result = children[i].Run();
            if (result != BTStatus.Failure) return result;
        }
        return BTStatus.Failure;
    }
}
