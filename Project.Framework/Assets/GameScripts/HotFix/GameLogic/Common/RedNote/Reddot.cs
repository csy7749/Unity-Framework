using UnityEngine;

namespace GameLogic.RedNote
{
    public class Reddot : UIWidget
    {
        private string path;
        public string Path
        {
            get => path;
            set
            {
                UnRegisterReddot();
                path = value;
                if (bAwaked)
                {
                    RegisterReddot();
                }
            }
        }

        private GameObject _reddotFlag;
        public GameObject ReddotFlag
        {
            get
            {
                if (_reddotFlag == null)
                {
                    _reddotFlag = transform.GetChild(0).gameObject;
                }
                return _reddotFlag;
            }
            set => _reddotFlag = value;
        }

        private bool bAwaked;

        protected override void OnRefresh()
        {
            base.OnRefresh();
            
            bAwaked = true;
            RegisterReddot();
            ReddotManager.RefreshShown(path);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            UnRegisterReddot();
        }

        public void SetReddotShow(bool isShown)
        {
            ReddotFlag?.SetActive(isShown);
        }

        private void RegisterReddot()
        {
            if (string.IsNullOrEmpty(Path))
            {
                return;
            }

            ReddotManager.RegisterRedDotUI(this);
        }

        private void UnRegisterReddot()
        {
            if (string.IsNullOrEmpty(Path))
            {
                return;
            }

            ReddotManager.UnRegisterRedDotUI(this);
        }
    }
}