
namespace MuffinSpace
{
	public struct ShaderSizeInfo
	{
		public ShaderSizeInfo(int sizeBytesP, int sizeElementsP)
		{
			sizeBytes = sizeBytesP;
			sizeElements = sizeElementsP;
		}

		public int sizeBytes;
		public int sizeElements;
	}

	public struct ShaderAttribute
	{
		public ShaderAttribute(ShaderAttributeName nameParam, ShaderDataType dataTypeParam)
		{
			name = nameParam;
			dataType = dataTypeParam;
			location = -1;
			elementSize = -1;
		}
		public ShaderAttribute(ShaderAttributeName nameParam, ShaderDataType dataTypeParam, int locationParam, int elementSizeParam)
		{
			name = nameParam;
			dataType = dataTypeParam;
			location = locationParam;
			elementSize = elementSizeParam;
		}

		public bool IsValid()
		{
			return name != ShaderAttributeName.InvalidAttributeName;
		}
		public ShaderAttributeName name;
		public ShaderDataType dataType;
		public int location;
		public int elementSize;
	}

	public struct ShaderUniform
	{
		public ShaderUniform(ShaderUniformName nameParam, ShaderDataType dataTypeParam
		, int locationParam = -1
		, ShaderUniformName arrayNameParam = ShaderUniformName.InvalidUniformName
		, int arrayIndexParam = -1)
		{
			name = nameParam;
			dataType = dataTypeParam;
			location = locationParam;
			arrayName = arrayNameParam;
			arrayIndex = arrayIndexParam;
		}

		public bool IsValid()
		{
			return name != ShaderUniformName.InvalidUniformName;
		}
		public ShaderUniformName name;
		public ShaderUniformName arrayName;
		public ShaderDataType dataType;
		public int location;
		public int arrayIndex;
	}

}