using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Managers
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private BombCounter _bombCount;
        [SerializeField] private GameObject _winUI;
        [SerializeField] private GameObject _loseUI;
        [SerializeField] private ParticleSystem _particle;

        [Header("Props")]
        [SerializeField] private GameObject countDown;
        [SerializeField] private GameObject bombCounter;
        [SerializeField] private List<GameObject> propsList;

        private float _timer;
        private int _spawnPropsWidth;
        private int _spawnPropsHeight;

        private void Awake()
        {
            _timer = 0;
            _spawnPropsWidth = 9;
            _spawnPropsHeight = 5;
            RandomPropsPosition(countDown);
            RandomPropsPosition(bombCounter);
            foreach (var props in propsList)
            {
                int _random = Random.Range(0, 100);
                if (_random < 50)
                {
                    Instantiate(props, new Vector3(Random.Range(-_spawnPropsWidth, _spawnPropsWidth), Random.Range(-_spawnPropsHeight, _spawnPropsHeight), gameObject.transform.position.z), quaternion.identity);
                    RandomPropsPosition(props);
                }
            }
        }

        private void Update()
        {
            _timer += Time.deltaTime;

            if (!(_timer >= 1)) return;

            _timer = 0;
        }

        private void RandomPropsPosition(GameObject gameObject)
        {
            gameObject.transform.position = new Vector3(Random.Range(-_spawnPropsWidth, _spawnPropsWidth), Random.Range(-_spawnPropsHeight, _spawnPropsHeight), gameObject.transform.position.z);
        }

        public void UpdateBombText(int bombCounter)
        {
            _bombCount.UpdateCounter(bombCounter);
        }

        public void ShowWinUI()
        {
            _winUI.SetActive(true);
            Instantiate<ParticleSystem>(_particle, transform,false);
            GetComponent<AudioSource>().Play();
        }

        public void ShowLoseUI()
        {
            _loseUI.SetActive(true);
        }

        public void LoadGameScene()
        {
            SceneLoader.LoadGameScene();
        }

        public void LoadLobbyScene()
        {
            SceneLoader.LoadLobbyScene();
        }
    }
}
