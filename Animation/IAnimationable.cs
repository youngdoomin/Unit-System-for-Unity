using System.Collections.Generic;

public interface IAnimationable
{
    // BaseAnimationController에 접근하여 필요한 애니메이션 정보 가지고 오기
    public void InitAnimation() { }
    public List<string> AniStringList { get; set; }
}
