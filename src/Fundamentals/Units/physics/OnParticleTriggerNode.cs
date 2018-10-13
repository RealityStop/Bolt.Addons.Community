using Ludiq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Bolt.Addons.Community.Fundamentals.Units.physics
{
    ///// <summary>
    ///// Called when a particle hits a trigger.
    ///// </summary>
    //[UnitCategory("Events/Physics")]
    //public sealed class OnParticleTriggerNode : GameObjectEventUnit<EmptyEventArgs>
    //{
    //    protected override string hookName => EventHooks.OnParticleCollision;


    //    public ValueInput Particles { get; private set; }

    //    [PortLabel("Enter")]
    //    [DoNotSerialize]
    //    private ValueOutput _enter;


    //    [PortLabel("Exit")]
    //    [DoNotSerialize]
    //    private ValueOutput _exit;


    //    [PortLabel("Inside")]
    //    [DoNotSerialize]
    //    private ValueOutput _inside;
        
    //    [PortLabel("Outside")]
    //    [DoNotSerialize]
    //    public ValueOutput _outside;

    //    protected override void Definition()
    //    {
    //        base.Definition();

    //        Particles = ValueInput<ParticleSystem>(nameof(Particles)).NullMeansSelf();

    //        _enter = ValueOutput<List<ParticleSystem.Particle>>(nameof(_enter));
    //        _exit = ValueOutput<List<ParticleSystem.Particle>>(nameof(_exit));
    //        _inside = ValueOutput<List<ParticleSystem.Particle>>(nameof(_inside));
    //        _outside = ValueOutput<List<ParticleSystem.Particle>>(nameof(_outside));
    //    }

    //    //Okay, we need to store the trigger particles list so that we can pass it to the GetTriggerParticlesCall.  We can't directly set value.


    //    protected override void AssignArguments(Flow flow, EmptyEventArgs dummyArgs)
    //    {
    //        var particles = flow.GetValue<ParticleSystem>(Particles);

    //        if (_enter.hasAnyConnection)
    //            flow.SetValue(this._enter, particles.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, )

    //        flow.SetValue(this.other, other);

    //        var collisionEvents = new List<ParticleCollisionEvent>();

    //        var data = flow.stack.GetElementData<Data>(this);

    //        data.target.GetComponent<ParticleSystem>().GetCollisionEvents(other, collisionEvents);

    //        flow.SetValue(this.collisionEvents, collisionEvents);
    //    }
    //}
}