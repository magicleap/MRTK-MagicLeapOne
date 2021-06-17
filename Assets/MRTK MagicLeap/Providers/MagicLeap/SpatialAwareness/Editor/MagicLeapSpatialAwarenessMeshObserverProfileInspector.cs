/// -------------------------------------------------------------------------------
// MRTK - MagicLeap
// https://github.com/magicleap/MRTK-MagicLeap
// -------------------------------------------------------------------------------
//
// MIT License
//
// Copyright(c) 2021 Magic Leap, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//
// -------------------------------------------------------------------------------
//

using MagicLeap.MRTK.SpatialAwareness;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Editor.SpatialAwareness;
using UnityEditor;

[CustomEditor(typeof(MagicLeapSpatialAwarenessMeshObserverProfile))]
public class MagicLeapSpatialAwarenessMeshObserverProfileInspector : MixedRealitySpatialAwarenessMeshObserverProfileInspector
{
    // Magic Leap Settings
    private SerializedProperty requestVertexConfidence;
    private SerializedProperty planarize;
    private SerializedProperty removeMeshSkirt;
    private SerializedProperty fillHoleLength;
    private SerializedProperty disconnectedComponentArea;
    private SerializedProperty batchSize;

    protected override void OnEnable()
    {
        base.OnEnable();

        // General settings
        requestVertexConfidence = serializedObject.FindProperty("RequestVertexConfidence");
        planarize = serializedObject.FindProperty("Planarize");
        removeMeshSkirt = serializedObject.FindProperty("RemoveMeshSkirt");
        fillHoleLength = serializedObject.FindProperty("FillHoleLength");
        disconnectedComponentArea = serializedObject.FindProperty("DisconnectedComponentArea");
        batchSize = serializedObject.FindProperty("BatchSize");

    }

    public override void OnInspectorGUI()
    {
       base.OnInspectorGUI();
       using (new EditorGUI.DisabledGroupScope(IsProfileLock((BaseMixedRealityProfile) target)))
       {
           EditorGUILayout.Space();
           EditorGUILayout.LabelField("Magic Leap Settings", EditorStyles.boldLabel);
           {
               EditorGUILayout.HelpBox("The values below will be used to initialize Magic Leap's Meshing system. " +
                                       "Runtime changes can be made using Magic Leap's MeshingSettings instance.",
                   MessageType.Info);
               EditorGUILayout.PropertyField(requestVertexConfidence);
               EditorGUILayout.Space();
               EditorGUILayout.PropertyField(planarize);
               EditorGUILayout.Space();
               EditorGUILayout.PropertyField(removeMeshSkirt);
               EditorGUILayout.Space();
               EditorGUILayout.PropertyField(fillHoleLength);
               EditorGUILayout.Space();
               EditorGUILayout.PropertyField(disconnectedComponentArea);
               EditorGUILayout.Space();
               EditorGUILayout.PropertyField(batchSize);
           }
           serializedObject.ApplyModifiedProperties();
       }

    }
}
