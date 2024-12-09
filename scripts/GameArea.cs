using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DefaultNamespace
{
    public class GameArea : MonoBehaviour
    {
        private Block this[int x, int y]
        {
            get => blocks.FirstOrDefault(block => block._x == x && block._y == y) ??
                   new Block { _x = -999, _y = -999, _value = -999 };
            set
            {
                if (value == null)
                {
                    Destroy(this[x, y].gameObject);
                    blocks.Remove(this[x, y]);
                    return;
                }

                value._x = x;
                value._y = y;
            }
        }

        //Задержка при спуске блока
        [SerializeField] private float delay = 1f;


        [SerializeField] private Block blockPrefab;

        //Размеры поля
        [SerializeField] private int _width;
        [SerializeField] private int _height;


        [SerializeField] private Material[] _material;
        
        private List<Block> blocks = new List<Block>();
        private Structure _structure;

        public void Construct(int width, int height)
        {
            _width = width;
            _height = height;
        }

        void Start()
        {
            //Создаём фигуру 2х1 вверху поля
            CreateFigure();
            StartCoroutine(Cycle());
        }

        private void Update()
        {
            Move();

            //Проверяем конец игры
            GameEnd();
        }

        private void GameEnd()
        {
            if (blocks.Any(block => block._y == _height && block._structure.isStatic))
                Application.Quit();
        }

        private void CreateFigure()
        {
            var structure = new Structure();
            CreateCoprimeNumbers(out int n1, out int n2);
            int randomX = Random.Range(0, _width - 1);

            Block b1 = Instantiate(blockPrefab, new Vector3(randomX, _height, 0), Quaternion.identity);
            Block b2 = Instantiate(blockPrefab, new Vector3(randomX + 1, _height, 0), Quaternion.identity);

            b1.Construct(randomX, _height, n1, structure);
            b2.Construct(randomX + 1, _height, n2, structure);

            blocks.Add(b1);
            blocks.Add(b2);
            _structure = structure;
            _structure.center = b1;
        }

        private  void CreateCoprimeNumbers(out int n1, out int n2)
        {
            n1 = Random.Range(2, 19);
            n2 = Random.Range(2, 19);
            while (!(FindGCD(n1, n2) == 1))
            {
                n2 = Random.Range(2, 19);
            }
        }


        private void Move()
        {
            //Сдвиг фигуры
            MoveRight();
            MoveLeft();

            //Спустить фигуру быстрее
            MoveDown();

            //Поворот фигуры на 90
            RotateFigure();
        }

        private void MoveRight()
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                foreach (var block in _structure.blocks.OrderByDescending(block => block._x))
                {
                    if (isExists(block) && CheckGridPosition(block._x + 1, block._y) || block._x >= _width)
                        return;
                }

                foreach (var block in _structure.blocks.OrderByDescending(block => block._x))
                {
                    block._x += 1;
                    block.transform.position = new Vector3(block.transform.position.x + 1,
                        block.transform.position.y, block.transform.position.z);
                }
            }
        }

        private void MoveLeft()
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                foreach (var block in _structure.blocks.OrderBy(block => block._x))
                {
                    if (isExists(block) && CheckGridPosition(block._x - 1, block._y) || block._x <= 0)
                        return;
                }

                foreach (var block in _structure.blocks.OrderBy(block => block._x))
                {
                    block._x -= 1;
                    block.transform.position = new Vector3(block.transform.position.x - 1,
                        block.transform.position.y, block.transform.position.z);
                }
            }
        }

        private void MoveDown()
        {
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                foreach (var block in _structure.blocks.OrderBy(block => block._y))
                {
                    if (isExists(block) && CheckStatic(block._x, block._y - 1) || block._y <= 0)
                        return;
                }


                foreach (var block in _structure.blocks.OrderBy(block => block._y))
                {
                    block._y -= 1;
                    block.transform.position = new Vector3(block.transform.position.x,
                        block.transform.position.y - 1, block.transform.position.z);
                }
            }
        }

        private void RotateFigure()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                List<Vector2> vector2 = new List<Vector2>();
                foreach (var block in _structure.blocks)
                {
                    var center = new Vector3(_structure.center._x, _structure.center._y);
                    var pos = new Vector3(block._x, block._y);
                    var local = pos - center;
                    var matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, -90), Vector3.one);
                    var pointer = matrix.MultiplyPoint(local) + center;
                    var tempX = (int)Math.Round(pointer.x);
                    var tempY = (int)Math.Round(pointer.y);
                    vector2.Add(new Vector2(tempX, tempY));
                    if (pointer == pos)
                        continue;
                    if (IsOutOfBounds(tempX, tempY) && CheckStatic(tempX, tempY)) return;
                }

                for (var i = 0; i < _structure.blocks.Count; i++)
                {
                    var currentBlock = _structure.blocks[i];
                    var currentPosition = vector2[i];

                    if (this[(int)currentPosition.x, (int)currentPosition.y]._value == -999)
                    {
                        currentBlock._x = (int)currentPosition.x;
                        currentBlock._y = (int)currentPosition.y;
                        currentBlock.transform.position = currentPosition;  
                    }
                    
                }
            }
        }

        private void MoveFigure()
        {
            foreach (var block in _structure.blocks.OrderBy(block => block._y))
            {
                if (isExists(block) && CheckStatic(block._x, block._y - 1) || block._y <= 0)
                {
                    CheckCollision();
                    this[(int)block.transform.position.x, (int)block.transform.position.y] = block;
                    block._structure.isStatic = true;
                    foreach (var structureBlock in _structure.blocks)
                    {
                        structureBlock.GetComponent<MeshRenderer>().material = _material[1];
                    }
                    
                    CreateFigure();
                    return;
                }

            }
            foreach (var block in _structure.blocks.OrderBy(block => block._y))
            {
                block._y -= 1;
                block.transform.position = new Vector3(block.transform.position.x,
                    block.transform.position.y - 1, block.transform.position.z);
            }
        }

        private bool IsOutOfBounds(int tempX, int tempY)
        {
            return tempX < 0 || tempX >= _width || tempY < 0 || tempY >= _height;
        }

        private bool CheckStatic(int tempX, int tempY)
        {
            return this[tempX, tempY]._structure.isStatic;
        }

        private void CheckCollision()
        {
            foreach (var block in _structure.blocks)
            {
                var i = block._x;
                var j = block._y;
                if ((block._value != this[i + 1, j]._value ||
                     block._value != this[i, j + 1]._value ||
                     block._value != this[i - 1, j]._value ||
                     block._value != this[i, j - 1]._value))
                {
                    DifferentValues(block, i, j); 
                }

                SameValues(block, i, j);

            }
        }

        private void SameValues(Block block, int i, int j)
        {
            if (block._value == this[block._x + 1, block._y]._value)
            {
                this[i, j] = null;
                this[i + 1, j] = null;
            }

            else if (block._value == this[i, j + 1]._value)
            {
                this[i, j] = null;
                this[i, j + 1] = null;
            }

            else if (block._value == this[i - 1, j]._value)
            {
                this[i, j] = null;
                this[i - 1, j] = null;
            }

            else if (block._value == this[i, j - 1]._value)
            {
                this[i, j] = null;
                this[i, j - 1] = null;
            }
        }

        private void DifferentValues(Block block, int i, int j)
        {
            int diriv;
            if (block._value != this[i + 1, j]._value && this[i + 1, j]._value != -999)
            {
                diriv = FindGCD(block._value, this[i + 1, j]._value);
                NewBlock(i, j, diriv);
                NewBlock(i + 1, j, diriv);
            }

            if (block._value != this[i, j + 1]._value && this[i, j + 1]._value != -999)
            {
                diriv = FindGCD(block._value, this[i, j + 1]._value);
                NewBlock(i, j, diriv);
                NewBlock(i, j + 1, diriv);
            }

            if (block._value != this[i - 1, j]._value && this[i - 1, j]._value != -999)
            {
                diriv = FindGCD(block._value, this[i - 1, j]._value);
                NewBlock(i, j, diriv);
                NewBlock(i - 1, j, diriv);
            }

            if (block._value != this[i, j - 1]._value && this[i, j - 1]._value != -999)
            {
                diriv = FindGCD(block._value, this[i, j - 1]._value);
                NewBlock(i, j, diriv);
                NewBlock(i, j - 1, diriv);
            }
        }

        private void NewBlock(int i, int j, int diriv)
        {
            this[i, j]._value /= diriv;
            this[i, j].text.text = this[i, j]._value.ToString();

            if (this[i, j]._value == 1)
            {
                this[i, j] = null;
            }
        }

        private int FindGCD(int a, int b)
        {
            for (int i = Math.Max(a, b); i > 0; i--)
            {
                if (a % i == 0 && b % i == 0)
                {
                    return i;
                }
            }

            return 1;
        }
        
        private bool isExists(Block block)
        {
            return this[block._x, block._y - 1]._value != -999;
        }

        private bool CheckGridPosition(int x, int y)
        {
            return blocks.Any(block => block._x == x && block._y == y);
        }

        private IEnumerator Delay(float f)
        {
            
            //Ждём
            yield return new WaitForSeconds(f);
        }

        private IEnumerator Cycle()
        {
            while (true)
            {
                //Двигаем фигуру вниз на 1
                MoveFigure();

                yield return Delay(delay);
            }
        }
        
    }
}