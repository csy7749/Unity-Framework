using UnityEngine;
using UnityEngine.UI;

namespace GameLogic.RedDotNew
{
    public class RedDotViewBase : MonoBehaviour
    {
        [SerializeField] protected int _redDotId = -1;
        [SerializeField] private GameObject _normalGo;
        [SerializeField] private GameObject _newGo;
        [SerializeField] private GameObject _numberGo;
        [SerializeField] private Text _textNumber;

        private RedDotConfigAsset.RedDotConfigData _redDotData;
        private string _path = string.Empty;

        protected virtual void Awake()
        {
            Init();
        }

        protected virtual void OnDestroy()
        {
            Unregister();
        }

        protected virtual void Init()
        {
            if (!TryGetConfig(_redDotId, out var config))
            {
                HideAll();
                return;
            }

            SetupVisual(config);
            if (ContainsFormatPlaceholder(config.Path))
            {
                ChangeRedDotCount(0);
                return;
            }

            _path = config.Path;
            RedDotTree.Instance.Register(_redDotId, ChangeRedDotCount);
        }

        public virtual void Register(int redDotId, params object[] parameters)
        {
            _redDotId = redDotId;
            Unregister();

            if (!TryGetConfig(_redDotId, out var config))
            {
                HideAll();
                return;
            }

            SetupVisual(config);
            _path = FormatPath(config.Path, parameters);
            RedDotTree.Instance.Register(_redDotId, ChangeRedDotCount, parameters);
        }

        public virtual void Watch()
        {
            if (_redDotId == -1 || string.IsNullOrWhiteSpace(_path))
            {
                return;
            }

            RedDotTree.Instance.Watch(_path);
        }

        public void Unregister()
        {
            if (!string.IsNullOrWhiteSpace(_path))
            {
                RedDotTree.Instance.Unregister(_path, ChangeRedDotCount);
                _path = string.Empty;
            }

            ChangeRedDotCount(0);
        }

        public virtual void ChangeRedDotCount(int count)
        {
            if (_redDotData == null)
            {
                HideAll();
                return;
            }

            var shown = count > 0;
            switch (_redDotData.ShowType)
            {
                case RedDotShowType.Normal:
                    _normalGo?.SetActive(shown);
                    break;
                case RedDotShowType.New:
                    _newGo?.SetActive(shown);
                    break;
                case RedDotShowType.Number:
                    _numberGo?.SetActive(shown);
                    if (_textNumber != null)
                    {
                        _textNumber.text = count.ToString();
                    }
                    break;
            }
        }

        private void SetupVisual(RedDotConfigAsset.RedDotConfigData config)
        {
            _redDotData = config;
            _normalGo?.SetActive(config.ShowType == RedDotShowType.Normal);
            _newGo?.SetActive(config.ShowType == RedDotShowType.New);
            _numberGo?.SetActive(config.ShowType == RedDotShowType.Number);
        }

        private bool TryGetConfig(int redDotId, out RedDotConfigAsset.RedDotConfigData config)
        {
            config = null;
            var configAsset = RedDotConfigAsset.Instance;
            if (configAsset?.DataDic == null)
            {
                return false;
            }

            return configAsset.DataDic.TryGetValue(redDotId, out config);
        }

        private static bool ContainsFormatPlaceholder(string path)
        {
            return path.Contains("{") || path.Contains("}");
        }

        private static string FormatPath(string pathTemplate, object[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
            {
                return pathTemplate;
            }

            return string.Format(pathTemplate, parameters);
        }

        private void HideAll()
        {
            _normalGo?.SetActive(false);
            _newGo?.SetActive(false);
            _numberGo?.SetActive(false);
        }
    }
}
