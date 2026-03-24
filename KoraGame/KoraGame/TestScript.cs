
namespace KoraGame
{
    internal class TestScript : ScriptableBehaviour
    {
        float rotateSpeed = 60f;

        protected override void OnUpdate()
        {
            GameObject.LocalRotation *= QuaternionF.Euler(rotateSpeed / 2f * Time.DeltaTime, rotateSpeed * Time.DeltaTime, rotateSpeed / 2f * Time.DeltaTime);

            //if (Input.GetMouseDown(Simple3D.Input.MouseButton.Left) == true)
            //    Console.WriteLine("Down: " + Input.MousePosition);

            //if (Input.GetMouseUp(Simple3D.Input.MouseButton.Left) == true)
            //    Console.WriteLine("Up");
        }
    }
}
