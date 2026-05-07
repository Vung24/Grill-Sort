using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Combo : MonoBehaviour
{
    [SerializeField] private GameObject _comboObject;
    [SerializeField] private Image _comboFillImage;
    [SerializeField] private TextMeshProUGUI _comboText;
    [SerializeField] private float _comboDuration = 10f;

    private int _currentCombo = 0;
    private float _timer = 0f;

    private void Start()
    {
        // Khởi tạo chắc chắn cấu hình thanh chạy UI cho Image từ Code
        if (_comboFillImage != null)
        {
            _comboFillImage.type = Image.Type.Filled;
            _comboFillImage.fillAmount = 0f;
        }
        
        ResetCombo();
    }

    void Update()
    {
        if (_currentCombo > 0)
        {
            _timer -= Time.deltaTime;
            
            if (_comboFillImage != null)
            {
                _comboFillImage.fillAmount = _timer / _comboDuration;
            }

            if (_timer <= 0)
            {
                ResetCombo();
            }
        }
    }

    public void AddCombo()
    {
        _currentCombo++;
        _timer = _comboDuration;

        if (_comboObject != null)
        {
            _comboObject.SetActive(true);
        }

        if (_comboText != null)
        {
            // Trạng thái combo text là không bị giới hạn số
            _comboText.text = "Combo x" + _currentCombo;
        }

        if (AudioManager.Instance != null)
        {
            // Gọi âm thanh giới hạn max là 8 trong logic của AudioManager
            AudioManager.Instance.PlayComboAudio(_currentCombo);
        }
    }

    public void ResetCombo()
    {
        _currentCombo = 0;
        _timer = 0f;

        if (_comboObject != null)
        {
            _comboObject.SetActive(false);
        }
        
        if (_comboText != null)
        {
            _comboText.text = "";
        }
        
        if (_comboFillImage != null)
        {
            _comboFillImage.fillAmount = 0f;
        }
    }
}
