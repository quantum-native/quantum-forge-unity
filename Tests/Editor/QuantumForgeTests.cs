using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using NUnit.Framework;
using QRG.QuantumForge.Core;
using UnityEngine;
using UnityEngine.TestTools;
using static UnityEngine.GraphicsBuffer;


public class QuantumForgeTests
{
    private QuantumForge.NativeQuantumProperty qubit1;
    private QuantumForge.NativeQuantumProperty qubit2;
    private QuantumForge.NativeQuantumProperty qutrit1;
    private QuantumForge.NativeQuantumProperty qutrit2;

    private QuantumForge.NativeQuantumProperty bellQubit1;
    private QuantumForge.NativeQuantumProperty bellQubit2;

    [SetUp]
    public void Setup()
    {
        qubit1 = new QuantumForge.NativeQuantumProperty(2);
        qubit2 = new QuantumForge.NativeQuantumProperty(2);
        qutrit1 = new QuantumForge.NativeQuantumProperty(3);
        qutrit2 = new QuantumForge.NativeQuantumProperty(3);

        bellQubit1 = new QuantumForge.NativeQuantumProperty(2);
        bellQubit2 = new QuantumForge.NativeQuantumProperty(2);
        QuantumForge.Hadamard(bellQubit1);
        QuantumForge.Cycle(bellQubit2, new[] { bellQubit1.is_value(1) });
    }

    [TearDown]
    public void Teardown()
    {
        qubit1.Dispose();
        qubit2.Dispose();
        qutrit1.Dispose();
        qutrit2.Dispose();
    }

    [Test]
    public void TestInitializeQuantumProperty()
    {
        var q = new QuantumForge.NativeQuantumProperty(3, 2);
        Assert.AreEqual(3, q.Dimension);
        Assert.AreEqual(2, QuantumForge.Measure(q)[0]);
    }

    [Test]
    public void TestCycle()
    {
        Assert.DoesNotThrow(() => QuantumForge.Cycle(qubit1));
        var m = QuantumForge.Measure(qubit1);
        Assert.AreEqual(m[0], 1);

        var p1 = qubit1.is_value(1);
        var p2 = qubit2.is_value(0);
        Assert.DoesNotThrow(()=> QuantumForge.Cycle(qutrit1, new []{ p1,p2}));
        m = QuantumForge.Measure(qutrit1);
        Assert.AreEqual(m[0], 1);

        p1 = qubit1.is_value(1); 
        p2 = qubit2.is_value(0);
        Assert.DoesNotThrow(() => QuantumForge.Cycle(qutrit1, new[] { p1, p2 }));
        m = QuantumForge.Measure(qutrit1);
        Assert.AreEqual(m[0], 2);

        Assert.DoesNotThrow(() => QuantumForge.Cycle(qutrit1));
        m = QuantumForge.Measure(qutrit1);
        Assert.AreEqual(m[0], 0);
    }

    [Test]
    public void TestShift()
    {
        Assert.DoesNotThrow(() => QuantumForge.Shift(qutrit1));
        var m = QuantumForge.Measure(qutrit1);
        Assert.AreEqual(m[0], 2);
    }

    [Test]
    public void TestFractionalShift()
    {
        var m = QuantumForge.Measure(qubit1);
        Assert.AreEqual(m[0], 0);
        Assert.DoesNotThrow(() => QuantumForge.Shift(qubit1, 1.0f / 3.0f));
        Assert.DoesNotThrow(() => QuantumForge.Shift(qubit1, 1.0f / 3.0f));
        Assert.DoesNotThrow(() => QuantumForge.Shift(qubit1, 1.0f / 3.0f));
        m = QuantumForge.Measure(qubit1);
        Assert.AreEqual(m[0], 1);
    }

    [Test]
    public void TestNCycle()
    {
        Assert.DoesNotThrow(() => QuantumForge.Hadamard(qutrit1));
        Assert.DoesNotThrow(() => QuantumForge.NCycle(qutrit1,qutrit2));
        var p = QuantumForge.Probabilities(qutrit1,qutrit2);
        for (int i = 0; i < 3; ++i)
        {
            var pii = Array.Find(p, bp =>
            {
                return bp.QuditValues.SequenceEqual(new[] { i, i });
            });
            Assert.IsNotNull(pii);
            Assert.AreEqual(1.0f/3.0f, pii.Probability);
        }

    }

    [Test]
    public void TestHadamard()
    {
        var predicate = qubit2.is_value(0);
        Assert.DoesNotThrow(() => QuantumForge.Hadamard(qubit1, predicate));

        var p = QuantumForge.Probabilities(qubit1);
        Assert.AreEqual(p.Length, 2);
        Assert.AreEqual(p[0].Probability, 0.5f);
        Assert.AreEqual(p[1].Probability, 0.5f);
    }

    [Test]
    public void TestInverseHadamard()
    {
        var predicate = qubit2.is_value(0);
        Assert.DoesNotThrow(() => QuantumForge.Hadamard(qubit1, predicate));
        Assert.DoesNotThrow(() => QuantumForge.InverseHadamard(qubit1, predicate));

        var p = QuantumForge.Probabilities(qubit1);
        Assert.AreEqual(p.Length, 2);
        Assert.AreEqual(p[0].Probability, 1.0f);
        Assert.AreEqual(p[1].Probability, 0.0f);
    }

    [Test]
    public void TestSwap()
    {
        Assert.DoesNotThrow(() => QuantumForge.Swap(qubit1, qubit2));
    }

    [Test]
    public void TestISwap()
    {
        Assert.DoesNotThrow(() => QuantumForge.ISwap(qubit1, qubit2));
    }

    [Test]
    public void TestFractionalISwap()
    {
        Assert.DoesNotThrow(() => QuantumForge.ISwap(qubit1, qubit2, 0.5f));
    }

    [Test]
    public void TestPredicatedISwap()
    {
        var predicate = qubit1.is_value(1);
        Assert.DoesNotThrow(() => QuantumForge.Cycle(qubit2));
        Assert.DoesNotThrow(() => QuantumForge.ISwap(qubit1, qubit2, 1.0f, predicate));

        var measurement = QuantumForge.Measure(qubit1, qubit2);
        Assert.AreEqual(0, measurement[0]);
        Assert.AreEqual(1, measurement[1]);

        Assert.DoesNotThrow(() => QuantumForge.Cycle(qubit1));
        Assert.DoesNotThrow(() => QuantumForge.Cycle(qubit2));
        Assert.DoesNotThrow(() => QuantumForge.ISwap(qubit1, qubit2, 1.0f, predicate));

        measurement = QuantumForge.Measure(qubit1, qubit2);
        Assert.AreEqual(0, measurement[0]);
        Assert.AreEqual(1, measurement[1]);
    }

    [Test]
    public void TestPhaseRotate()
    {
        var predicate = qubit1.is_value(1);
        Assert.DoesNotThrow(() => QuantumForge.PhaseRotate(45.0f, predicate));
    }

    [Test]
    public void TestClock()
    {
        QuantumForge.Hadamard(qubit1);
        QuantumForge.Clock(qubit1);
        QuantumForge.InverseHadamard(qubit1);
        var p = QuantumForge.Probabilities(qubit1);
        Assert.AreEqual(p.Length, 2);
        Assert.AreEqual(p[0].Probability, 0.0f);
        Assert.AreEqual(p[1].Probability, 1.0f);

        p = QuantumForge.Probabilities(qutrit1);
        Assert.AreEqual(p[0].Probability, 1.0f);
        Assert.AreEqual(p[1].Probability, 0.0f);
        Assert.AreEqual(p[2].Probability, 0.0f);

        QuantumForge.Hadamard(qutrit1);
        QuantumForge.Clock(qutrit1);
        QuantumForge.InverseHadamard(qutrit1);
        p = QuantumForge.Probabilities(qutrit1);
        Assert.AreEqual(p.Length, 3);
        Assert.AreEqual(p[0].Probability, 0.0f);
        Assert.AreEqual(p[1].Probability, 0.0f);
        Assert.AreEqual(p[2].Probability, 1.0f);

        QuantumForge.Hadamard(qutrit1);
        QuantumForge.Clock(qutrit1);
        QuantumForge.InverseHadamard(qutrit1);
        p = QuantumForge.Probabilities(qutrit1);
        Assert.AreEqual(p.Length, 3);
        Assert.AreEqual(p[0].Probability, 0.0f);
        Assert.AreEqual(p[1].Probability, 1.0f);
        Assert.AreEqual(p[2].Probability, 0.0f);

        QuantumForge.Hadamard(qutrit1);
        QuantumForge.Clock(qutrit1);
        QuantumForge.InverseHadamard(qutrit1);
        p = QuantumForge.Probabilities(qutrit1);
        Assert.AreEqual(p.Length, 3);
        Assert.AreEqual(p[0].Probability, 1.0f);
        Assert.AreEqual(p[1].Probability, 0.0f);
        Assert.AreEqual(p[2].Probability, 0.0f);
    }

    [Test]
    public void TestReset()
    {
        Assert.DoesNotThrow(() => QuantumForge.Cycle(qutrit1));
        Assert.DoesNotThrow(() => QuantumForge.Cycle(qutrit1));
        var m = QuantumForge.Measure(qutrit1);
        Assert.AreEqual(2, m[0]);

        Assert.DoesNotThrow(() => QuantumForge.Reset(qutrit1, m[0]));
        m = QuantumForge.Measure(qutrit1);
        Assert.AreEqual(0, m[0]);
    }

    [Test]
    public void TestFractionalHadamardRequiresUpdatedPlugin()
    {
        Assert.Throws<NotSupportedException>(() => QuantumForge.Hadamard(qubit1, 0.5f));
    }

    [Test]
    public void TestMeasure()
    {
        var result = QuantumForge.Measure(new[] { qubit1, qubit2 });
        Assert.AreEqual(2, result.Length);
    }

    [Test]
    public void TestPredicatedMeasure()
    {
        var result = QuantumForge.Measure(new[] { qubit1.is_value(0), qubit2.is_not_value(1) });
        Assert.AreEqual(result,1);
    }

    [Test]
    public void TestReducedDensityMatrix()
    {
        Assert.DoesNotThrow(() => QuantumForge.Hadamard(qubit1));
        var predicate = qubit1.is_value(1);
        Assert.DoesNotThrow(() => QuantumForge.Cycle(qubit2, predicate));
        var rdm = QuantumForge.ReducedDensityMatrix(new[] { qubit1, qubit2 });
        Assert.AreEqual(16, rdm.Length);
        for(int i=0; i<4; ++i)
        {
            for (int j = 0; j < 4; ++j)
            {
                if ((i == 0 && j==0) || (i == 0 && j == 3)|| (i == 3 && j == 0) || (i == 3 && j == 3))
                {
                    Assert.AreEqual(new Complex(0.5, 0.0), rdm[i,j]);
                }
                else
                {
                    Assert.AreEqual(new Complex(0.0, 0.0), rdm[i,j]);
                }
            }
        }
    }

    [Test]
    public void TestMutualInformation()
    {
        Assert.DoesNotThrow(() => QuantumForge.Hadamard(qubit1));
        var predicate = qubit1.is_value(1);
        Assert.DoesNotThrow(() => QuantumForge.Cycle(qubit2, predicate));
        var mi = QuantumForge.MutualInformation(new[] { qubit1, qubit2 });
        Assert.AreEqual(2, mi.Length);
        float target = 2.0f * MathF.Log(2.0f);
        Assert.IsTrue(Mathf.Approximately(target, mi[0]));
    }

    [Test]
    public void TestCorrelationMatrix()
    {
        var m = QuantumForge.CorrelationMatrix(new[] { bellQubit1, bellQubit2 });
        Assert.AreEqual(2, m.GetLength(0));
        Assert.AreEqual(2, m.GetLength(1));
        Assert.IsTrue(Mathf.Approximately(1.0f, m[0,0]));
        Assert.IsTrue(Mathf.Approximately(-1.0f, m[0, 1]));
        Assert.IsTrue(Mathf.Approximately(-1.0f, m[1, 0]));
        Assert.IsTrue(Mathf.Approximately(1.0f, m[1, 1]));

        m = QuantumForge.CorrelationMatrix(new[] { qubit1, qubit2 });
        Debug.Log(ToString(m));
        Assert.AreEqual(2, m.GetLength(0));
        Assert.AreEqual(2, m.GetLength(1));
        Assert.IsTrue(Mathf.Approximately(0.0f, m[0, 0]));
        Assert.IsTrue(Mathf.Approximately(0.0f, m[0, 1]));
        Assert.IsTrue(Mathf.Approximately(0.0f, m[1, 0]));
        Assert.IsTrue(Mathf.Approximately(0.0f, m[1, 1]));
        
    }

    private string ToString(float[,] m)
    {
        string result = "";
        for (int i = 0; i < m.GetLength(0); i++)
        {
            for (int j = 0; j < m.GetLength(1); j++)
            {
                result += m[i, j].ToString("0.00") + " ";
            }
            result += "\n";
        }

        return result;
    }
}
