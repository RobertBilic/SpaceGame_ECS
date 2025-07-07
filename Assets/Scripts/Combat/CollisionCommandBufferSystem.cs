using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace SpaceGame
{
	[UpdateInGroup(typeof(CombatCollisionGroup), OrderLast = true)]
	public partial class CollisionCommandBufferSystem : EntityCommandBufferSystem
	{
		protected override void OnCreate()
		{
			base.OnCreate();
			this.RegisterSingleton<Singleton>(ref base.PendingBuffers, base.World.Unmanaged);

		}

		public struct Singleton : IComponentData, IQueryTypeParameter, IECBSingleton
		{
			public unsafe EntityCommandBuffer CreateCommandBuffer(WorldUnmanaged world)
			{
				return EntityCommandBufferSystem.CreateCommandBuffer(ref *this.pendingBuffers, this.allocator, world);
			}

			public unsafe void SetPendingBufferList(ref UnsafeList<EntityCommandBuffer> buffers)
			{
				this.pendingBuffers = (UnsafeList<EntityCommandBuffer>*)UnsafeUtility.AddressOf<UnsafeList<EntityCommandBuffer>>(ref buffers);
			}

			public void SetAllocator(Allocator allocatorIn)
			{
				this.allocator = allocatorIn;
			}

			public void SetAllocator(AllocatorManager.AllocatorHandle allocatorIn)
			{
				this.allocator = allocatorIn;
			}

			internal unsafe UnsafeList<EntityCommandBuffer>* pendingBuffers;

			internal AllocatorManager.AllocatorHandle allocator;
		}
	}
}
