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

#my $N = Mesh->new;
# these stay always?
my $M = Mesh->new;
my $MUV = Mesh->new;
# these get culled
my $NP = Mesh->new;
my $TP = Mesh->new;
my $NPC = Mesh->new;
my $TPC = Mesh->new;

my $roundDetails = 4;
my $margin = 0.09;
my $edge = 0.01;


#my $start = $circle->Extrude($backA, 1)->ScaleCentric(1.2);
#$start = $start->Extrude($ringA, 1)->MoveBy(V(0,0,-$margin));

my $dim = 0.06;
my $rotation = PI/2;
my $lenParts = 2**$roundDetails;
my $seg = $rotation / $lenParts;
my $axis = V(1,0,0);
my $origin = V(0,$margin,$margin);

my $circle = Circle->new($dim, $roundDetails);
$circle = $circle->MoveBy(V(0,0,$margin));
my $end = $circle; my $uv = $end->clone;

my $start = Circle->new($dim, $roundDetails);
$start = $start->MoveBy(V(0,0,$margin));
$start = $start->MoveBy(V(0,0,-$edge));
$start = $start->Extrude($NP, 0)->ScaleCentric(1.2);
$start = $start->Extrude($NPC, 0)->MoveBy(V(0,0,$edge));
$start = $start->CloseArea($NP, 0);

for (my $i = 0; $i < $lenParts; $i++)
{
	$uv = $uv->Extrude($MUV, 1)->MoveBy(V(0,0,0.1));
	$end = $end->Extrude($M, 1)->Rotate($seg,$axis,$origin);
}

$end = $end->clone()->MoveBy(V(0,-$edge,0));
$end = $end->Extrude($TP, 1)->ScaleCentric(1.2);
$end = $end->Extrude($TPC, 1)->MoveBy(V(0,$edge,0));
$end = $end->CloseArea($TP, 1);

$M->MoveBy(V(0.5, 1 - $margin, - $margin));
$NP->MoveBy(V(0.5, 1 - $margin, - $margin));
$TP->MoveBy(V(0.5, 1 - $margin, - $margin));
$NPC->MoveBy(V(0.5, 1 - $margin, - $margin));
$TPC->MoveBy(V(0.5, 1 - $margin, - $margin));

# $TP->InvertFaceNormals();

$M->InterpolateNormalsFromFaces(0);
$NPC->SetNormalsFromFaces(0);
$TPC->SetNormalsFromFaces(0);
$MUV->SetNormalsFromFaces(0);
$NP->SetNormalsFromFaces(0);
$TP->SetNormalsFromFaces(0);

$NPC->CylindricUVs(V(0,0,1));
$TPC->CylindricUVs(V(0,1,0));

$MUV->CylindricUVs(V(0,0,1));
if (scalar($M->Faces) == scalar($MUV->Faces))
{
	for (my $i = 0; $i < scalar($M->Faces); $i += 1)
	{
		$M->Face($i)->{uvs} = $MUV->Face($i)->{uvs};
	}
}

#$M->ProjectUVs(V(0.25,0.75,0.95));
$NP->ProjectUVs(V(0,0,1));
$TP->ProjectUVs(V(0,1,0));


#$NP->CopyFaces($NPC->Faces);
#$TP->CopyFaces($TPC->Faces);

push @{$NP->{faces}}, @{$NPC->{faces}};
push @{$TP->{faces}}, @{$TPC->{faces}};

#$NP->Merger($NPC, 0);

# $M->MoveBy(V(0.5 - $dim, 0.5 - $margin, 0.5 - $margin));
# $M->MoveBy(V(0.5, 0.5, 0.5));

# $M->MoveBy(V(0, $dim, $dim));

my $PATH = 'G:\\7daysmodding\\ModdingA20\\OcbAutoHarvest\\Unity\\AutoHarvest\\assets\\Models\\wall-corner-small\\';

$M->WriteObj("${PATH}/M.obj", "M");
$NP->WriteObj("${PATH}/NP.obj", "NP");
$TP->WriteObj("${PATH}/TP.obj", "TP");


my $meshes = Meshes->new;
$meshes->AddMesh($M);
$meshes->AddMesh($NP);
$meshes->AddMesh($TP);


#$meshes->AddMesh(Cube->new(1)->MoveBy(V(0,0,0)));
$meshes->WriteObj("wall-corner-edge.obj", "WallCornerEdge");

my $box = $meshes->CalcBoundingBox();
print "Bounding Box ", $box->[0], " - ", $box->[1], "\n";

__DATA__

my $pipe_top = Mesh->read("import/pipe_straight/Mesh/T.obj")->ScaleBy(10000);
my $pipe_front = Mesh->read("import/pipe_straight/Mesh/N_P.obj")->ScaleBy(10000);
my $pipe_back = Mesh->read("import/pipe_straight/Mesh/S_P.obj")->ScaleBy(10000);

$pipe_top->Rotate(PI/2,V(0,1,0),V(0,0.5,0.5))->MoveBy(V(0,0,-.5));
$pipe_front->Rotate(PI/2,V(0,1,0),V(0,0.5,0.5))->MoveBy(V(0,0,-.5));
$pipe_back->Rotate(PI/2,V(0,1,0),V(0,0.5,0.5))->MoveBy(V(0,0,-.5));

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
