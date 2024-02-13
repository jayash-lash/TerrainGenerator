using ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;

namespace ProceduralGeneration.Scripts.BlockTypeUI
{
    public class BlockPanelUI : MonoBehaviour
    {
        [SerializeField] private BlockType[] _blockTypes;
        [SerializeField] private Button[] _blockButtons;

        private int selectedBlockIndex = 0;

        private void Start()
        {
            for (int i = 0; i < _blockButtons.Length; i++)
            {
                int index = i;
                _blockButtons[i].onClick.AddListener(() => OnBlockButtonClick(index));
            }
        }

        private void Update()
        {
            for (int i = 1; i <= 9; i++)
            {
                if (Input.GetKeyDown(i.ToString()))
                {
                    SelectBlock(i - 1); 
                }
            }
        }

        private void OnBlockButtonClick(int blockIndex)
        {
            SelectBlock(blockIndex);
        }

        private void SelectBlock(int blockIndex)
        {
            _blockButtons[selectedBlockIndex].interactable = true;
            selectedBlockIndex = Mathf.Clamp(blockIndex, 0, _blockTypes.Length - 1);
            _blockButtons[selectedBlockIndex].interactable = false;
        }

        public BlockType GetSelectedBlockType()
        {
            return _blockTypes[selectedBlockIndex];
        }
    }
}