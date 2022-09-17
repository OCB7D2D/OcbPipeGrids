use strict;
use warnings;
use lib "lib";
use constant PI => 4 * atan2(1, 1);
use Math::Vector::Real;
use Meshes;
use Circle;
use Curve;
use Pipes;
use Cube;

my $pipe_top = Mesh->read("import/pipe_straight/Mesh/T.obj")->ScaleBy(10000);
my $pipe_front = Mesh->read("import/pipe_straight/Mesh/N_P.obj")->ScaleBy(10000);
my $pipe_back = Mesh->read("import/pipe_straight/Mesh/S_P.obj")->ScaleBy(10000);

$pipe_top->Rotate(PI/2,V(0,1,0),V(0,0.5,0.5))->MoveBy(V(0,0,-.5));
$pipe_front->Rotate(PI/2,V(0,1,0),V(0,0.5,0.5))->MoveBy(V(0,0,-.5));
$pipe_back->Rotate(PI/2,V(0,1,0),V(0,0.5,0.5))->MoveBy(V(0,0,-.5));

my $N = Mesh->new;
my $S = Mesh->new;
my $T = Mesh->new;
my $NP = Mesh->new;
my $SP = Mesh->new;
my $BP = Mesh->new;

my $curve = Curve->new(-1, 1, 4, 4.5)->Scale(V(-0.625,-1,-1));
# Move to ground so we can scale height and width
$curve = $curve->MoveBy(V(0,1,-0.15))->Scale(V(0.8,0.975,1));

sub CreateScaledPart
{
	return $_[0]->Extrude($_[1], $_[2])->Scale(V($_[3],$_[3],1));
}

# $part1->InterpolateNormalsFromFaces();

#$path = CreateScaledPart($path, $part1, 1, 1.2);

my $scale0 = V(0.9,0.9,1);
my $scale1 = V(0.9,0.95,1);

#	$pipe->UnwrapUV();
#	$S->UnwrapUV3();
#	$ring->UnwrapUV2();


my $start = $curve->Extrude($S, 1)->Scale($scale0);
$start = $start->Extrude($T, 1, $BP)->MoveBy(V(0,0,-0.1));
$start = $start->Extrude($S, 1)->Scale($scale1);
$start = $start->Extrude($T, 1, $BP)->MoveBy(V(0,0,-0.05));

my $end = $curve->Extrude($T, 0, $BP)->MoveBy(V(0,0,0.3));
$end = $end->Extrude($N, 0)->Scale($scale0);
$end = $end->Extrude($T, 0, $BP)->MoveBy(V(0,0,0.1));
$end = $end->Extrude($N, 0)->Scale($scale1);
$end = $end->Extrude($T, 0, $BP)->MoveBy(V(0,0,0.05));

$start = $start->CloseArea($S, 1);
$end = $end->CloseArea($N, 0);

my $scale = V(1,1,1);
my $move = V(0,-0.5,0);

$N = $N->clone;
$N->Scale($scale);
$N->MoveBy($move);
$S = $S->clone;
$S->Scale($scale);
$S->MoveBy($move);
$T = $T->clone;
$T->Scale($scale);
$T->MoveBy($move);
$BP = $BP->clone;
$BP->Scale($scale);
$BP->MoveBy($move);

$N->SetNormalsFromFaces(0);
$S->SetNormalsFromFaces(0);
$BP->SetNormalsFromFaces(1);

$T->ProjectUVs(V(1,0.75,1));
# Implement proper uv unwrapping
# Note: this can wait til later
# $T->CylindricUVs(V(0,0,1));

$S->ProjectUVs(V(0,0,1));
$N->ProjectUVs(V(0,0,-1));
$BP->ProjectUVs(V(0,-1,0));



$T->CopyFaces($pipe_top);
$T->CopyFaces($N);
$T->CopyFaces($S);

$SP->CopyFaces($pipe_back);
$NP->CopyFaces($pipe_front);

$T->WriteObj("pipe-pump/T.obj", "T");
#$S->WriteObj("pipe-pump/S.obj", "S");
#$N->WriteObj("pipe-pump/N.obj", "N");
$SP->WriteObj("pipe-pump/S_P.obj", "S_P");
$NP->WriteObj("pipe-pump/N_P.obj", "N_P");
$BP->WriteObj("pipe-pump/B_P.obj", "B_P");

my $meshes = Meshes->new;
#$meshes->AddMesh($N);
#$meshes->AddMesh($S);
$meshes->AddMesh($T);
$meshes->AddMesh($NP);
$meshes->AddMesh($SP);
$meshes->AddMesh($BP);

#$meshes->AddMesh(Cube->new(1)->MoveBy(V(0,0,0)));
$meshes->WriteObj("pipe-pump.obj", "PipePump");

my $box = $meshes->CalcBoundingBox();
print "Bounding Box ", $box->[0], " - ", $box->[1], "\n";