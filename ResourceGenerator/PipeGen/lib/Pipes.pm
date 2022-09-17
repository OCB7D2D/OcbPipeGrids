package Pipes;
use Math::Vector::Real;
use constant PI => 4 * atan2(1, 1);
my $roundDetails = 2;
my $rotation = PI/2;
my $margin = 0.05;

sub CreateStraight
{
	my $meshes = Meshes->new;

	my $ring = Mesh->new();
	my $back = Mesh->new();
	my $pipe = Mesh->new();

	my $circle = Circle->new(0.25, $roundDetails);
	$circle = $circle->MoveBy(V(0,0,$margin));

	my $start = $circle->Extrude($back, 1, 2)->ScaleCentric(1.2);
	$start = $start->Extrude($ring, 1)->MoveBy(V(0,0,-$margin));

	my $end = $circle->Extrude($pipe, 0, 2)->MoveBy(V(0,0,0.5-$margin));
	#$pipe->SetInterpolation(1);
	# $mesh->FaceNormal();

	$pipe->UnwrapUV();
	$back->UnwrapUV3();
	$ring->UnwrapUV2();

	$back->InterpolateNormalsFromFaces();
	$ring->InterpolateNormalsFromFaces();
	$pipe->InterpolateNormalsFromFaces();

	$meshes->AddMesh($pipe);
	$meshes->AddMesh($back);
	$meshes->AddMesh($ring);
	return $meshes;
}
