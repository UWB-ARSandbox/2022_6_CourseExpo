using System;
using UnityEngine;
using ASL;

/// <summary>A builder class to build ASLParticleLists.  These lists represent a position/color pair list of points for a point cloud or particlesystem instance update.</summary>
public class ASLParticleListBuilder
{
    private ParticleSystem.Particle[] _particles;
    private Color _defaultColor;
    private bool _useCustomColor;
    private bool _colorReady;
    private bool _positionReady;
    private Color[] _colors;
    private Vector3[] _positions;

    /// <summary>
    /// ASLParticleListBuilder constructor.  Constructs a new particle list builder.
    /// </summary>
    /// <param name="defaultColor">Parameter for the default color if custom colors are not used.  Uses a default of Color.green.</param>
    public ASLParticleListBuilder(Color? defaultColor = null)
    {
        _defaultColor = defaultColor ?? Color.green;
        _colorReady = false;
        _positionReady = false;
        _useCustomColor = false;
    }

    /// <summary>
    /// Field to get the count of Particles in the Particle array.
    /// </summary>
    /// <returns>An integer representing the count of particles in the built particle array.  Returns 0 if no particles exist.</returns>
    public int Count
    {
        get
        {
            if (_particles == null)
            {
                return 0;
            }
            else
            {
                return _particles.Length;
            }
        }
    }

    /// <summary>
    /// Method to get the completed state of the Builder.
    /// </summary>
    /// <returns>A bool representing if the particle system is complete and ready for retriveval.</returns>
    public bool IsComplete
    {
        get
        {
            return (_positionReady && !_useCustomColor) || (_colorReady && _positionReady);
        }
    }

    /// <summary>
    /// Method to get custom color usage.
    /// </summary>
    /// <returns>A bool representing custom color usage.</returns>
    public bool getUseCustomColor()
    {
        return _useCustomColor;
    }

    /// <summary>
    /// Method to set custom color usage
    /// </summary>
    /// <param name="useCustomColor">Bool to set custom color usage.  Set to true if passing in color data.</param>
    public void setUseCustomColor(bool useCustomColor)
    {
        _useCustomColor = useCustomColor;
    }

    /// <summary>
    /// The built particle list for the ASLParticleListBuilder 
    /// </summary>
    /// <returns>A particle list with position and color data.  Will return null if the particle list is invalid or not completed.</returns>
    public ParticleSystem.Particle[] Particles
    {
        get
        {
            if (IsComplete)
            {
                if (_particles == null)
                {
                    _particles = new ParticleSystem.Particle[_positions.Length];
                    for (int i = 0; i < _positions.Length; i++)
                    {
                        _particles[i].position = _positions[i];
                        _particles[i].startColor = _useCustomColor ? _colors[i] : _defaultColor;
                        _particles[i].startSize = .015f;
                        _particles[i].startLifetime = 1000000;
                        _particles[i].remainingLifetime = 1000000;
                    }
                }
                return _particles;
            }
            else
            {
                return null;
            }
        }
    }
    /// <summary>
    /// Method to set the particle's Vector3 positions in the particle array.
    /// </summary>
    /// <param name="rawPointArray">A raw array representing the particle points.  Must match size of the color array if using custom colors.</param>
    public void SetParticlePoints(float[] rawPointArray)
    {
        _positions = GameLiftManager.ConvertFloatArrayToVector3Array(rawPointArray);
        if (_useCustomColor && _colors != null && _colors.Length != _positions.Length)
        {
            throw new ArgumentException("ASLParticleList:Exception: Particle array does not match color array size.");
        }
        _positionReady = true;
    }

    /// <summary>
    /// Method to set the particle's colors in the particle array.
    /// </summary>
    /// <param name="rawColorArray">A raw array representing the particle Color colors.  Must match size of the points array if using custom colors.</param>
    public void SetParticleColors(float[] rawColorArray)
    {
        _colors = GameLiftManager.ConvertFloatArrayToColorArray(rawColorArray);
        if (_positions != null && _colors.Length != _positions.Length)
        {
            throw new ArgumentException("ASLParticleList:Exception: Particle array does not match color array size.");
        }
        _colorReady = true;
    }
}
