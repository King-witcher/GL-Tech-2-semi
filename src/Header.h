#pragma once

#define external __declspec(dllexport)
constexpr auto TODEGREE = 57.2957795131f;
constexpr auto TORAD = 0.01745329251f;
typedef int pixel;


struct Texture32_
{
	pixel* buffer;
	int height;
	int width;
};

struct Material_
{
	float hoffset;
	float hrepeat;
	Texture32_ texture;
	float voffset;
	float vrepeat;

	pixel MapPixel(float, float);
};

struct Vector
{
	float X;
	float Y;
};

struct WallData
{
	Vector geom_direction;
	Vector geom_start;
	Material_ material;
};

struct Sprite_
{
	Vector position;
	Material_ material;
};

struct SceneData
{
	Sprite_** sprities;
	int sprite_count;
	int sprite_max;
	WallData** walls;
	int wall_count;
	int wall_max;
	Material_ background;
};

struct Ray
{
	Vector Direction;
	Vector Start;

	Ray(Vector, float);

	void GetCollisionData(WallData*, float&, float&);
	WallData* NearestWall(SceneData*, float&, float&);
};

struct RenderStruct
{
	pixel* bitmap_buffer;
	int bitmap_height;
	int bitmap_width;
	float* cache_angles;
	float cache_colHeight1;
	float* cache_cosines;
	float camera_angle;
	float camera_HFOV;
	Vector camera_position;
	SceneData* scene;
};

extern "C"
{
	external void NativeRender(RenderStruct&);
	external void SayHello()
	{
		cout << endl << "Hello" << endl;
	}
}