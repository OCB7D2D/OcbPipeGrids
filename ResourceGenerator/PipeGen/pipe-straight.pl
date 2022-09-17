use strict;
use warnings;
use lib "lib";
use constant PI => 4 * atan2(1, 1);
use Math::Vector::Real;
use Triangle;
use Vertices;
use Circle;
use Meshes;

my $roundDetails = 4;
my $rotation = PI/2;
my $margin = 0.05;
my $radius = 0.1;


sub CreateLid
{
	my $lid = Mesh->new();
	my $circle = Circle->new($radius, $roundDetails);
	$circle = $circle->ScaleCentric(1.2)->MoveBy(V(0,0.5,0));
	$circle->Extrude($lid, 0)->SetPositions(0,0.5,0);
	$lid->SimplifyFaces;
	#$lid->SetNormalsFromFaces;
	return $lid;
}


sub CreatePipe
{
	my $meshes = Meshes->new;

	my $ring = Mesh->new();
	my $back = Mesh->new();
	my $pipe = Mesh->new();

	my $lid1 = Mesh->new();
	my $lid2 = Mesh->new();

	my $circle = Circle->new($radius, $roundDetails);
	$circle = $circle->MoveBy(V(0,0.5,$margin));

	my $start = $circle->Extrude($back, 1)->ScaleCentric(1.2);
	$start = $start->Extrude($ring, 1)->MoveBy(V(0,0,-$margin));

	my $end = $circle->Extrude($pipe, 0)->MoveBy(V(0,0,1-$margin));
	#$pipe->SetInterpolation(1);
	# $mesh->FaceNormal();

	$pipe->UnwrapUV();
	$back->UnwrapUV3();
	$ring->UnwrapUV2();

	$end->CloseArea($lid1, 0);
	$start->CloseArea($lid2, 1);

	$back->InterpolateNormalsFromFaces();
	$ring->InterpolateNormalsFromFaces();
	$pipe->InterpolateNormalsFromFaces();
	$lid1->InterpolateNormalsFromFaces();
	$lid2->InterpolateNormalsFromFaces();

	$lid1->ProjectUVs(V(0,0,1), 0.5);
	$lid2->ProjectUVs(V(0,0,1), 0.5);

	$meshes->AddMesh($pipe);
	$meshes->AddMesh($back);
	$meshes->AddMesh($ring);
	$meshes->AddMesh($lid1);
	$meshes->AddMesh($lid2);
	return $meshes;
}

my $lid1 = CreateLid()->Rotate(PI/2*0,V(0,1,0),V(0,0.5,0.5));
my $lid2 = CreateLid()->Rotate(PI/2*2,V(0,1,0),V(0,0.5,0.5));
my $tube1 = CreatePipe()->Rotate(PI/2*0,V(0,1,0),V(0,0.5,0.5));

warn "Merging started\n";

my $group = Meshes->new;
$group->AddMesh($tube1->{meshes}->[0]);

foreach my $face ($group->Faces)
{
	delete $face->{normal};
}

$group->InterpolateNormalsFromFaces();

# $tube3->Merge($tube2);
# $tube3->Merge($tube4);
warn "Merge finished\n";

my $mesh = Meshes->new;
$mesh->AddMesh($tube1);
#$mesh->AddMesh($lid1);
#$mesh->AddMesh($lid2);

open(my $fh, ">", "pipe-straight.obj");
print $fh "o PipeStraight\n";
print $fh $mesh->ToWaveFrontObj();


# $extruded->Debug();

